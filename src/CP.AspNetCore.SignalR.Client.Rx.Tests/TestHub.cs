// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.AspNetCore.SignalR.Client.Rx.Reactive.Tests;
#else
namespace CP.AspNetCore.SignalR.Client.Rx.Tests;
#endif

/// <summary>SignalR hub used by the observable extension tests.</summary>
/// <param name="events">The server-side test event sink.</param>
public sealed class TestHub(TestHubEvents events) : Hub
{
    /// <summary>The number of values emitted by the fixed-length stream.</summary>
    private const int CountStreamLength = 3;

    /// <summary>Sends a parameterless callback to the caller.</summary>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public Task SendPing() => Clients.Caller.SendAsync("Ping");

    /// <summary>Sends a single value callback to the caller.</summary>
    /// <param name="value">The value to echo.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public Task SendEcho(string value) => Clients.Caller.SendAsync("Echo", value);

    /// <summary>Sends a two value callback to the caller.</summary>
    /// <param name="left">The first value.</param>
    /// <param name="right">The second value.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public Task SendPair(string left, string right) => Clients.Caller.SendAsync("Pair", left, right);

    /// <summary>Sends a variable number of integer callback arguments to the caller.</summary>
    /// <param name="methodName">The callback method name.</param>
    /// <param name="count">The number of one-based values to send.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    public Task SendValues(string methodName, int count)
    {
        var values = new object?[count];
        for (var index = 0; index < count; index++)
        {
            values[index] = index + 1;
        }

        return Clients.Caller.SendCoreAsync(methodName, values);
    }

    /// <summary>Aborts the current connection so automatic reconnect behavior can be observed.</summary>
    public void AbortConnection() => Context.Abort();

    /// <summary>Returns the supplied value.</summary>
    /// <param name="value">The value to return.</param>
    /// <returns>A task that returns the supplied value.</returns>
    public Task<string> EchoValue(string value)
    {
        events.ObserveConnection(Context.ConnectionId);
        return Task.FromResult(value);
    }

    /// <summary>Stores a notification for the test to read.</summary>
    /// <param name="value">The notification value.</param>
    /// <returns>A task that represents the asynchronous write operation.</returns>
    public Task Store(string value) => events.WriteNotificationAsync(value).AsTask();

    /// <summary>Streams three values using async enumerable streaming.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An async sequence of values.</returns>
    public async IAsyncEnumerable<int> CountStream([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        events.ObserveConnection(Context.ConnectionId);

        for (var value = 1; value <= CountStreamLength; value++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return value;
            await Task.Yield();
        }
    }

    /// <summary>Streams values using channel-based streaming.</summary>
    /// <param name="count">The number of values to stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A channel reader for the streamed values.</returns>
    public ChannelReader<int> CountChannel(int count, CancellationToken cancellationToken)
    {
        events.ObserveConnection(Context.ConnectionId);

        var channel = Channel.CreateUnbounded<int>();

        _ = Task.Run(
            async () =>
            {
                try
                {
                    for (var value = 1; value <= count; value++)
                    {
                        await channel.Writer.WriteAsync(value, cancellationToken);
                    }

                    channel.Writer.Complete();
                }
                catch (Exception ex)
                {
                    channel.Writer.Complete(ex);
                }
            },
            cancellationToken);

        return channel.Reader;
    }
}
