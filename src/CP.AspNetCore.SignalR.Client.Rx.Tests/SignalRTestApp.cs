// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CP.AspNetCore.SignalR.Client.Rx.Tests;

/// <summary>Hosts an in-memory SignalR app for tests.</summary>
internal sealed class SignalRTestApp : IAsyncDisposable
{
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
    public HubConnection Connection { get; }

    /// <summary>Gets server-side test events.</summary>
    public TestHubEvents Events { get; }

    /// <summary>Creates and starts the in-memory SignalR host.</summary>
    /// <returns>A task that returns the test app.</returns>
    public static async Task<SignalRTestApp> CreateAsync()
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddSignalR(options => options.EnableDetailedErrors = true);
                        services.AddSingleton<TestHubEvents>();
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseEndpoints(endpoints => endpoints.MapHub<TestHub>("/testHub"));
                    });
            })
            .StartAsync();

        var server = host.GetTestServer();
        var connection = new HubConnectionBuilder()
            .WithUrl("http://localhost/testHub", options => options.HttpMessageHandlerFactory = _ => server.CreateHandler())
            .Build();
        var events = host.Services.GetRequiredService<TestHubEvents>();

        return new SignalRTestApp(host, connection, events);
    }

    /// <summary>Disposes the client connection and in-memory host.</summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await Connection.DisposeAsync();
        await _host.StopAsync();
        _host.Dispose();
    }
}
