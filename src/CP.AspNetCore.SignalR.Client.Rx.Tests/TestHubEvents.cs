// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.AspNetCore.SignalR.Client.Rx.Reactive.Tests;
#else
namespace CP.AspNetCore.SignalR.Client.Rx.Tests;
#endif

/// <summary>Stores server-side events observed by tests.</summary>
public sealed class TestHubEvents
{
    /// <summary>The channel containing server notifications.</summary>
    private readonly Channel<string> _notifications = Channel.CreateUnbounded<string>();

    /// <summary>Gets the last observed SignalR connection identifier.</summary>
    public string? LastConnectionId { get; private set; }

    /// <summary>Records the current SignalR connection identifier.</summary>
    /// <param name="connectionId">The connection identifier.</param>
    public void ObserveConnection(string? connectionId) => LastConnectionId = connectionId;

    /// <summary>Writes a notification value.</summary>
    /// <param name="value">The notification value.</param>
    /// <returns>A value task that represents the asynchronous write operation.</returns>
    public ValueTask WriteNotificationAsync(string value) => _notifications.Writer.WriteAsync(value);

    /// <summary>Reads the next notification value.</summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that returns the next notification value.</returns>
    public async Task<string> ReadNotificationAsync(CancellationToken cancellationToken) => await _notifications.Reader.ReadAsync(cancellationToken);
}
