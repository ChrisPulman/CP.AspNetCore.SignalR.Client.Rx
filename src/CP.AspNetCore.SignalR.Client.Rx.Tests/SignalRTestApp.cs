// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.AspNetCore.SignalR.Client.Rx.Reactive.Tests;
#else
namespace CP.AspNetCore.SignalR.Client.Rx.Tests;
#endif

/// <summary>Hosts an in-memory SignalR app for tests.</summary>
internal sealed class SignalRTestApp : IAsyncDisposable
{
    /// <summary>The in-memory test host.</summary>
    private readonly IHost _host;

    /// <summary>Initializes a new instance of the <see cref="SignalRTestApp"/> class.</summary>
    /// <param name="host">The in-memory host.</param>
    /// <param name="connection">The SignalR client connection.</param>
    /// <param name="events">The server-side test event sink.</param>
    private SignalRTestApp(IHost host, HubConnection connection, TestHubEvents events)
    {
        _host = host;
        Connection = connection;
        Events = events;
    }

    /// <summary>Gets the SignalR client connection.</summary>
    internal HubConnection Connection { get; }

    /// <summary>Gets server-side test events.</summary>
    internal TestHubEvents Events { get; }

    /// <summary>Creates and starts the in-memory SignalR host.</summary>
    /// <returns>A task that returns the test app.</returns>
    internal static async Task<SignalRTestApp> CreateAsync()
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(static webBuilder =>
            {
                _ = webBuilder
                    .UseTestServer()
                    .ConfigureServices(static services =>
                    {
                        _ = services.AddSignalR(static options => options.EnableDetailedErrors = true);
                        _ = services.AddSingleton<TestHubEvents>();
                    })
                    .Configure(static app =>
                    {
                        _ = app.UseRouting();
                        _ = app.UseEndpoints(static endpoints => _ = endpoints.MapHub<TestHub>("/testHub"));
                    });
            })
            .StartAsync()
            .ConfigureAwait(false);

        var server = host.GetTestServer();
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/testHub", options => options.HttpMessageHandlerFactory = _ => server.CreateHandler())
            .Build();
        var events = host.Services.GetRequiredService<TestHubEvents>();

        return new(host, connection, events);
    }

    /// <summary>Disposes the client connection and in-memory host.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        await Connection.DisposeAsync().ConfigureAwait(false);
        await _host.StopAsync().ConfigureAwait(false);
        _host.Dispose();
    }
}
