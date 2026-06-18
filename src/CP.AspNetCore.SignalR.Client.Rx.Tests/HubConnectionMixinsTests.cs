// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace CP.AspNetCore.SignalR.Client.Rx.Tests;

/// <summary>Tests observable hub connection extension methods.</summary>
public sealed class HubConnectionMixinsTests
{
    /// <summary>Verifies that start, ensure-started, and stop observables drive the connection state.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StartEnsureStartedAndStopObservablesDriveConnectionState()
    {
        await using var app = await SignalRTestApp.CreateAsync();

        await app.Connection.StartObservable().ToTask().WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(app.Connection.State).IsEqualTo(HubConnectionState.Connected);

        await app.Connection.EnsureStarted().ToTask().WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(app.Connection.State).IsEqualTo(HubConnectionState.Connected);

        await app.Connection.StopObservable().ToTask().WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(app.Connection.State).IsEqualTo(HubConnectionState.Disconnected);
    }

    /// <summary>Verifies that hub callbacks are exposed as observable values.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task OnMethodsExposeHubCallbacksAsObservableValues()
    {
        await using var app = await SignalRTestApp.CreateAsync();
        var ping = new TaskCompletionSource<Unit>(TaskCreationOptions.RunContinuationsAsynchronously);
        var echo = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pair = new TaskCompletionSource<(string t1, string t2)>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var pingSubscription = app.Connection.On("Ping").Subscribe(value => ping.TrySetResult(value));
        using var echoSubscription = app.Connection.On<string>("Echo").Subscribe(value => echo.TrySetResult(value));
        using var pairSubscription = app.Connection.On<string, string>("Pair").Subscribe(value => pair.TrySetResult(value));

        await app.Connection.StartAsync().WaitAsync(TimeSpan.FromSeconds(5));

        await app.Connection.InvokeAsync("SendPing").WaitAsync(TimeSpan.FromSeconds(5));
        await app.Connection.InvokeAsync("SendEcho", "hello").WaitAsync(TimeSpan.FromSeconds(5));
        await app.Connection.InvokeAsync("SendPair", "left", "right").WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(await ping.Task.WaitAsync(TimeSpan.FromSeconds(5))).IsEqualTo(Unit.Default);
        await Assert.That(await echo.Task.WaitAsync(TimeSpan.FromSeconds(5))).IsEqualTo("hello");

        var actualPair = await pair.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(actualPair.t1).IsEqualTo("left");
        await Assert.That(actualPair.t2).IsEqualTo("right");
    }

    /// <summary>Verifies that invoke and send helpers forward hub method calls.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task InvokeAndSendObservablesForwardHubCalls()
    {
        await using var app = await SignalRTestApp.CreateAsync();

        await app.Connection.StartAsync().WaitAsync(TimeSpan.FromSeconds(5));

        var echo = await app.Connection.InvokeObservable<string>("EchoValue", default, "from-invoke")
            .ToTask()
            .WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(echo).IsEqualTo("from-invoke");

        await app.Connection.InvokeObservable("Store", default, "from-non-generic-invoke")
            .ToTask()
            .WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(await app.Events.ReadNotificationAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5)))
            .IsEqualTo("from-non-generic-invoke");

        await app.Connection.SendObservable("Store", default, "from-send")
            .ToTask()
            .WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(await app.Events.ReadNotificationAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(5)))
            .IsEqualTo("from-send");
    }

    /// <summary>Verifies that stream helpers expose async enumerable and channel-based hub streams.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StreamObservablesExposeHubStreams()
    {
        await using var app = await SignalRTestApp.CreateAsync();

        await app.Connection.StartAsync().WaitAsync(TimeSpan.FromSeconds(5));

        var asyncEnumerableValues = await app.Connection.StreamObservable<int>("CountStream")
            .ToList()
            .ToTask()
            .WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(asyncEnumerableValues.Count).IsEqualTo(3);
        await Assert.That(asyncEnumerableValues[0]).IsEqualTo(1);
        await Assert.That(asyncEnumerableValues[1]).IsEqualTo(2);
        await Assert.That(asyncEnumerableValues[2]).IsEqualTo(3);

        var channelValues = await app.Connection.StreamObservable<int>("CountChannel", default, 4)
            .ToList()
            .ToTask()
            .WaitAsync(TimeSpan.FromSeconds(5));
        await Assert.That(channelValues.Count).IsEqualTo(4);
        await Assert.That(channelValues[0]).IsEqualTo(1);
        await Assert.That(channelValues[1]).IsEqualTo(2);
        await Assert.That(channelValues[2]).IsEqualTo(3);
        await Assert.That(channelValues[3]).IsEqualTo(4);
    }

    /// <summary>Verifies that state helpers emit the current state and wait for matching values.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StateHelpersEmitCurrentStateAndWaitForRequestedState()
    {
        await using var app = await SignalRTestApp.CreateAsync();

        await app.Connection.StartAsync().WaitAsync(TimeSpan.FromSeconds(5));

        var currentState = await app.Connection.StateChanges()
            .FirstAsync()
            .ToTask()
            .WaitAsync(TimeSpan.FromSeconds(5));
        var waitedState = await app.Connection.WaitForState(HubConnectionState.Connected)
            .ToTask()
            .WaitAsync(TimeSpan.FromSeconds(5));

        await Assert.That(currentState).IsEqualTo(HubConnectionState.Connected);
        await Assert.That(waitedState).IsEqualTo(HubConnectionState.Connected);
    }
}
