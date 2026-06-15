// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Channels;

namespace CP.AspNetCore.SignalR.Client.Rx.Tests;

/// <summary>Stores server-side events observed by tests.</summary>
public sealed class TestHubEvents
{
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
