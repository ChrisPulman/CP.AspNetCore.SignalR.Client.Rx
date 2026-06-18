// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace CP.AspNetCore.SignalR.Client.Rx.Tests;

/// <summary>Tests observable start triggers.</summary>
public sealed class OnMixinsTests
{
    /// <summary>Verifies that observable hub connection sources start and emit their connections.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableConnectionSourceStartsAndEmitsConnection()
    {
        await using var app = await SignalRTestApp.CreateAsync();

        var startedConnection = await Observable.Return(app.Connection)
            .Start()
            .ToTask()
            .WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(startedConnection).IsSameReferenceAs(app.Connection);
        await Assert.That(startedConnection.State).IsEqualTo(HubConnectionState.Connected);
    }

    /// <summary>Verifies that arbitrary observable values can trigger starting a supplied connection.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableTriggerStartsSuppliedConnection()
    {
        await using var app = await SignalRTestApp.CreateAsync();

        var startedConnection = await Observable.Return("trigger")
            .Start(app.Connection)
            .ToTask()
            .WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(startedConnection).IsSameReferenceAs(app.Connection);
        await Assert.That(startedConnection.State).IsEqualTo(HubConnectionState.Connected);
    }
}
