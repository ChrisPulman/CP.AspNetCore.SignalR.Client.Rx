// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.AspNetCore.SignalR.Client.Rx.Reactive.Tests;
#else
namespace CP.AspNetCore.SignalR.Client.Rx.Tests;
#endif

/// <summary>Tests observable hub connection extension methods.</summary>
public sealed class HubConnectionMixinsTests
{
    /// <summary>A generic test hub method name.</summary>
    private const string ValueMethod = "Value";

    /// <summary>The test hub method that emits variable-arity callbacks.</summary>
    private const string SendValuesMethod = "SendValues";

    /// <summary>The number of values emitted by the async-enumerable stream.</summary>
    private const int AsyncStreamLength = 3;

    /// <summary>The number of values emitted by the channel stream.</summary>
    private const int ChannelStreamLength = 4;

    /// <summary>The index of the second stream value.</summary>
    private const int SecondIndex = 1;

    /// <summary>The second stream value.</summary>
    private const int SecondValue = 2;

    /// <summary>The index of the third stream value.</summary>
    private const int ThirdIndex = 2;

    /// <summary>The third stream value.</summary>
    private const int ThirdValue = 3;

    /// <summary>The index of the fourth stream value.</summary>
    private const int FourthIndex = 3;

    /// <summary>The fourth stream value.</summary>
    private const int FourthValue = 4;

    /// <summary>The fifth callback value.</summary>
    private const int FifthValue = 5;

    /// <summary>The sixth callback value.</summary>
    private const int SixthValue = 6;

    /// <summary>The seventh callback value.</summary>
    private const int SeventhValue = 7;

    /// <summary>The eighth callback value.</summary>
    private const int EighthValue = 8;

    /// <summary>The backing field for the closed event.</summary>
    private static readonly FieldInfo ClosedEventField = GetEventField(nameof(HubConnection.Closed));

    /// <summary>The backing field for the reconnecting event.</summary>
    private static readonly FieldInfo ReconnectingEventField = GetEventField(nameof(HubConnection.Reconnecting));

    /// <summary>The backing field for the reconnected event.</summary>
    private static readonly FieldInfo ReconnectedEventField = GetEventField(nameof(HubConnection.Reconnected));

    /// <summary>The maximum duration of an asynchronous test operation.</summary>
    private static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(5);

    /// <summary>Verifies that start, ensure-started, and stop observables drive the connection state.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StartEnsureStartedAndStopObservablesDriveConnectionState()
    {
        await using var app = await SignalRTestApp.CreateAsync();

        await app.Connection.StartObservable().ToTask().WaitAsync(TestTimeout);
        await Assert.That(app.Connection.State).IsEqualTo(HubConnectionState.Connected);

        await app.Connection.EnsureStarted().ToTask().WaitAsync(TestTimeout);
        await Assert.That(app.Connection.State).IsEqualTo(HubConnectionState.Connected);

        await app.Connection.StopObservable().ToTask().WaitAsync(TestTimeout);
        await Assert.That(app.Connection.State).IsEqualTo(HubConnectionState.Disconnected);
    }

    /// <summary>Verifies every connection extension rejects a null receiver immediately.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ConnectionExtensionsRejectNullReceivers()
    {
        HubConnection connection = null!;
        var method = new HubMethod<int>(ValueMethod);

        await Assert.That(() => connection.StartObservable()).Throws<ArgumentNullException>();
        await Assert.That(() => connection.StartObservable(default)).Throws<ArgumentNullException>();
        await Assert.That(() => connection.StartObservable(1, default)).Throws<ArgumentNullException>();
        await Assert.That(() => connection.EnsureStarted()).Throws<ArgumentNullException>();
        await Assert.That(() => connection.StopObservable()).Throws<ArgumentNullException>();
        await Assert.That(() => connection.StopObservable(default)).Throws<ArgumentNullException>();
        await Assert.That(() => connection.StopObservable(1, default)).Throws<ArgumentNullException>();
        await Assert.That(() => connection.StreamObservable(method, default)).Throws<ArgumentNullException>();
        await Assert.That(() => connection.StreamObservable(method, default, [])).Throws<ArgumentNullException>();
        await Assert.That(() => connection.InvokeObservable(method, default)).Throws<ArgumentNullException>();
        await Assert.That(() => connection.InvokeObservable(ValueMethod, default)).Throws<ArgumentNullException>();
        await Assert.That(() => connection.SendObservable(ValueMethod, default)).Throws<ArgumentNullException>();
        await Assert.That(() => connection.HasClosed()).Throws<ArgumentNullException>();
        await Assert.That(() => connection.IsReconnecting()).Throws<ArgumentNullException>();
        await Assert.That(() => connection.HasReconnected()).Throws<ArgumentNullException>();
        await Assert.That(() => connection.StateChanges()).Throws<ArgumentNullException>();
        await Assert.That(() => connection.WaitForState(HubConnectionState.Connected)).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies explicit cancellation and retry overloads preserve connection lifecycle behavior.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task CancellationAndRetryOverloadsDriveConnectionLifecycle()
    {
        await using var app = await SignalRTestApp.CreateAsync();

        await app.Connection.EnsureStarted(default).ToTask().WaitAsync(TestTimeout);
        await app.Connection.StopObservable(default).ToTask().WaitAsync(TestTimeout);

        await app.Connection.StartObservable(null, default).ToTask().WaitAsync(TestTimeout);
        await app.Connection.StopObservable(1, default).ToTask().WaitAsync(TestTimeout);

        var emptyStart = await app.Connection.StartObservable(0, default).ToList().ToTask().WaitAsync(TestTimeout);
        await Assert.That(emptyStart.Count).IsEqualTo(0);
        await Assert.That(app.Connection.State).IsEqualTo(HubConnectionState.Disconnected);

        await app.Connection.StartObservable(1, default).ToTask().WaitAsync(TestTimeout);
        await app.Connection.StopAsync().WaitAsync(TestTimeout);
    }

    /// <summary>Verifies all observable source and trigger start overloads emit the started connection.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task ObservableStartOverloadsEmitStartedConnections()
    {
        await using var app = await SignalRTestApp.CreateAsync();

        var sourceWithToken = await Observable.Return(app.Connection).Start((CancellationToken)default).ToTask().WaitAsync(TestTimeout);
        await Assert.That(sourceWithToken).IsSameReferenceAs(app.Connection);
        await app.Connection.StopAsync().WaitAsync(TestTimeout);

        var sourceWithRetry = await Observable.Return(app.Connection).Start(1, default).ToTask().WaitAsync(TestTimeout);
        await Assert.That(sourceWithRetry).IsSameReferenceAs(app.Connection);
        await app.Connection.StopAsync().WaitAsync(TestTimeout);

        var triggerWithToken = await Observable.Return("trigger").Start(app.Connection, default).ToTask().WaitAsync(TestTimeout);
        await Assert.That(triggerWithToken).IsSameReferenceAs(app.Connection);
        await app.Connection.StopAsync().WaitAsync(TestTimeout);

        var triggerWithRetry = await Observable.Return("trigger").Start(app.Connection, 1, default).ToTask().WaitAsync(TestTimeout);
        await Assert.That(triggerWithRetry).IsSameReferenceAs(app.Connection);
    }

    /// <summary>Verifies that hub callbacks are exposed as observable values.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task OnMethodsExposeHubCallbacksAsObservableValues()
    {
        await using var app = await SignalRTestApp.CreateAsync();
        var ping = new TaskCompletionSource<RxVoid>(TaskCreationOptions.RunContinuationsAsynchronously);
        var echo = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        var pair = new TaskCompletionSource<(string t1, string t2)>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var pingSubscription = app.Connection.On("Ping").Subscribe(value => ping.TrySetResult(value));
        using var echoSubscription = app.Connection.On(new HubMethod<string>("Echo")).Subscribe(value => echo.TrySetResult(value));
        using var pairSubscription = app.Connection.On(new HubMethod<(string, string)>("Pair")).Subscribe(value => pair.TrySetResult(value));

        await app.Connection.StartAsync().WaitAsync(TestTimeout);

        await app.Connection.InvokeAsync("SendPing").WaitAsync(TestTimeout);
        await app.Connection.InvokeAsync("SendEcho", "hello").WaitAsync(TestTimeout);
        await app.Connection.InvokeAsync("SendPair", "left", "right").WaitAsync(TestTimeout);

        await Assert.That(await ping.Task.WaitAsync(TestTimeout)).IsEqualTo(RxVoid.Default);
        await Assert.That(await echo.Task.WaitAsync(TestTimeout)).IsEqualTo("hello");

        var actualPair = await pair.Task.WaitAsync(TestTimeout);
        await Assert.That(actualPair.t1).IsEqualTo("left");
        await Assert.That(actualPair.t2).IsEqualTo("right");
    }

    /// <summary>Verifies that every supported callback arity emits its strongly typed tuple.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task OnMethodsExposeAllSupportedTupleArities()
    {
        await using var app = await SignalRTestApp.CreateAsync();
        var triple = new TaskCompletionSource<(int t1, int t2, int t3)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var quadruple = new TaskCompletionSource<(int t1, int t2, int t3, int t4)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var quintuple = new TaskCompletionSource<(int t1, int t2, int t3, int t4, int t5)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var sextuple = new TaskCompletionSource<(int t1, int t2, int t3, int t4, int t5, int t6)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var septuple = new TaskCompletionSource<(int t1, int t2, int t3, int t4, int t5, int t6, int t7)>(TaskCreationOptions.RunContinuationsAsynchronously);
        var octuple = new TaskCompletionSource<(int t1, int t2, int t3, int t4, int t5, int t6, int t7, int t8)>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var tripleSubscription = app.Connection.On(new HubMethod<(int, int, int)>("Triple")).Subscribe(value => triple.TrySetResult(value));
        using var quadrupleSubscription = app.Connection.On(new HubMethod<(int, int, int, int)>("Quadruple")).Subscribe(value => quadruple.TrySetResult(value));
        using var quintupleSubscription = app.Connection.On(new HubMethod<(int, int, int, int, int)>("Quintuple")).Subscribe(value => quintuple.TrySetResult(value));
        using var sextupleSubscription = app.Connection.On(new HubMethod<(int, int, int, int, int, int)>("Sextuple")).Subscribe(value => sextuple.TrySetResult(value));
        using var septupleSubscription = app.Connection.On(new HubMethod<(int, int, int, int, int, int, int)>("Septuple")).Subscribe(value => septuple.TrySetResult(value));
        using var octupleSubscription = app.Connection.On(new HubMethod<(int, int, int, int, int, int, int, int)>("Octuple")).Subscribe(value => octuple.TrySetResult(value));

        await app.Connection.StartAsync().WaitAsync(TestTimeout);
        await app.Connection.InvokeAsync(SendValuesMethod, "Triple", ThirdValue).WaitAsync(TestTimeout);
        await app.Connection.InvokeAsync(SendValuesMethod, "Quadruple", FourthValue).WaitAsync(TestTimeout);
        await app.Connection.InvokeAsync(SendValuesMethod, "Quintuple", FifthValue).WaitAsync(TestTimeout);
        await app.Connection.InvokeAsync(SendValuesMethod, "Sextuple", SixthValue).WaitAsync(TestTimeout);
        await app.Connection.InvokeAsync(SendValuesMethod, "Septuple", SeventhValue).WaitAsync(TestTimeout);
        await app.Connection.InvokeAsync(SendValuesMethod, "Octuple", EighthValue).WaitAsync(TestTimeout);

        await Assert.That((await triple.Task.WaitAsync(TestTimeout)).t3).IsEqualTo(ThirdValue);
        await Assert.That((await quadruple.Task.WaitAsync(TestTimeout)).t4).IsEqualTo(FourthValue);
        await Assert.That((await quintuple.Task.WaitAsync(TestTimeout)).t5).IsEqualTo(FifthValue);
        await Assert.That((await sextuple.Task.WaitAsync(TestTimeout)).t6).IsEqualTo(SixthValue);
        await Assert.That((await septuple.Task.WaitAsync(TestTimeout)).t7).IsEqualTo(SeventhValue);
        await Assert.That((await octuple.Task.WaitAsync(TestTimeout)).t8).IsEqualTo(EighthValue);
    }

    /// <summary>Verifies that invoke and send helpers forward hub method calls.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task InvokeAndSendObservablesForwardHubCalls()
    {
        await using var app = await SignalRTestApp.CreateAsync();

        await app.Connection.StartAsync().WaitAsync(TestTimeout);

        var echo = await app.Connection.InvokeObservable(new HubMethod<string>("EchoValue"), default, "from-invoke")
            .ToTask()
            .WaitAsync(TestTimeout);
        await Assert.That(echo).IsEqualTo("from-invoke");

        await app.Connection.InvokeObservable("Store", default, "from-non-generic-invoke")
            .ToTask()
            .WaitAsync(TestTimeout);
        await Assert.That(await app.Events.ReadNotificationAsync(CancellationToken.None).WaitAsync(TestTimeout))
            .IsEqualTo("from-non-generic-invoke");

        await app.Connection.SendObservable("Store", default, "from-send")
            .ToTask()
            .WaitAsync(TestTimeout);
        await Assert.That(await app.Events.ReadNotificationAsync(CancellationToken.None).WaitAsync(TestTimeout))
            .IsEqualTo("from-send");
    }

    /// <summary>Verifies that stream helpers expose async enumerable and channel-based hub streams.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StreamObservablesExposeHubStreams()
    {
        await using var app = await SignalRTestApp.CreateAsync();

        await app.Connection.StartAsync().WaitAsync(TestTimeout);

        var asyncEnumerableValues = await app.Connection.StreamObservable(new HubMethod<int>("CountStream"), default)
            .ToList()
            .ToTask()
            .WaitAsync(TestTimeout);
        await Assert.That(asyncEnumerableValues.Count).IsEqualTo(AsyncStreamLength);
        await Assert.That(asyncEnumerableValues[0]).IsEqualTo(1);
        await Assert.That(asyncEnumerableValues[SecondIndex]).IsEqualTo(SecondValue);
        await Assert.That(asyncEnumerableValues[ThirdIndex]).IsEqualTo(ThirdValue);

        var channelValues = await app.Connection.StreamObservable(new HubMethod<int>("CountChannel"), default, ChannelStreamLength)
            .ToList()
            .ToTask()
            .WaitAsync(TestTimeout);
        await Assert.That(channelValues.Count).IsEqualTo(ChannelStreamLength);
        await Assert.That(channelValues[0]).IsEqualTo(1);
        await Assert.That(channelValues[SecondIndex]).IsEqualTo(SecondValue);
        await Assert.That(channelValues[ThirdIndex]).IsEqualTo(ThirdValue);
        await Assert.That(channelValues[FourthIndex]).IsEqualTo(FourthValue);
    }

    /// <summary>Verifies stream helpers translate cancellation to completion and propagate server errors.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StreamObservablesHandleCancellationAndErrors()
    {
        await using var app = await SignalRTestApp.CreateAsync();
        await app.Connection.StartAsync().WaitAsync(TestTimeout);
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var cancelledAsyncStream = await app.Connection.StreamObservable(new HubMethod<int>("CountStream"), cancellation.Token)
            .ToList()
            .ToTask()
            .WaitAsync(TestTimeout);
        var cancelledChannelStream = await app.Connection.StreamObservable(new HubMethod<int>("CountChannel"), cancellation.Token, ChannelStreamLength)
            .ToList()
            .ToTask()
            .WaitAsync(TestTimeout);

        await Assert.That(cancelledAsyncStream.Count).IsEqualTo(0);
        await Assert.That(cancelledChannelStream.Count).IsEqualTo(0);

        var asyncStreamError = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        var channelStreamError = new TaskCompletionSource<Exception>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var asyncErrorSubscription = app.Connection.StreamObservable(new HubMethod<int>("MissingAsyncStream"), default)
            .Subscribe(static _ => { }, error => asyncStreamError.TrySetResult(error));
        using var channelErrorSubscription = app.Connection.StreamObservable(new HubMethod<int>("MissingChannelStream"), default, ChannelStreamLength)
            .Subscribe(static _ => { }, error => channelStreamError.TrySetResult(error));

        await Assert.That(await asyncStreamError.Task.WaitAsync(TestTimeout)).IsTypeOf<HubException>();
        await Assert.That(await channelStreamError.Task.WaitAsync(TestTimeout)).IsTypeOf<HubException>();
    }

    /// <summary>Verifies that state helpers emit the current state and wait for matching values.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task StateHelpersEmitCurrentStateAndWaitForRequestedState()
    {
        await using var app = await SignalRTestApp.CreateAsync();

        await app.Connection.StartAsync().WaitAsync(TestTimeout);

        var currentState = await app.Connection.StateChanges()
            .FirstAsync()
            .ToTask()
            .WaitAsync(TestTimeout);
        var waitedState = await app.Connection.WaitForState(HubConnectionState.Connected)
            .ToTask()
            .WaitAsync(TestTimeout);

        await Assert.That(currentState).IsEqualTo(HubConnectionState.Connected);
        await Assert.That(waitedState).IsEqualTo(HubConnectionState.Connected);
    }

    /// <summary>Verifies closed, reconnecting, and reconnected callbacks flow through their observable adapters.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task LifecycleEventObservablesEmitConnectionTransitions()
    {
        await using var app = await SignalRTestApp.CreateAsync();
        var closed = new TaskCompletionSource<Exception?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reconnecting = new TaskCompletionSource<Exception?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reconnected = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var reconnectingError = new InvalidOperationException("Reconnect requested.");
        const string ReconnectedId = "reconnected-id";

        using var closedSubscription = app.Connection.HasClosed().Subscribe(error => closed.TrySetResult(error));
        using var reconnectingSubscription = app.Connection.IsReconnecting().Subscribe(error => reconnecting.TrySetResult(error));
        using var reconnectedSubscription = app.Connection.HasReconnected().Subscribe(connectionId => reconnected.TrySetResult(connectionId));
        using var stateSubscription = app.Connection.StateChanges().Subscribe(static state => Debug.WriteLine(state));

        await RaiseEventAsync(ReconnectingEventField, app.Connection, reconnectingError);
        await RaiseEventAsync(ReconnectedEventField, app.Connection, ReconnectedId);
        await RaiseEventAsync(ClosedEventField, app.Connection, (Exception?)null);

        await Assert.That(await reconnecting.Task.WaitAsync(TestTimeout)).IsSameReferenceAs(reconnectingError);
        await Assert.That(await reconnected.Task.WaitAsync(TestTimeout)).IsEqualTo(ReconnectedId);
        await Assert.That(await closed.Task.WaitAsync(TestTimeout)).IsNull();
    }

    /// <summary>Gets a non-public SignalR event backing field.</summary>
    /// <param name="eventName">The event name.</param>
    /// <returns>The event backing field.</returns>
    private static FieldInfo GetEventField(string eventName) =>
        typeof(HubConnection).GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException(typeof(HubConnection).FullName, eventName);

    /// <summary>Raises a registered SignalR lifecycle event.</summary>
    /// <typeparam name="T">The event argument type.</typeparam>
    /// <param name="eventField">The event backing field.</param>
    /// <param name="connection">The connection whose handlers are invoked.</param>
    /// <param name="argument">The event argument.</param>
    /// <returns>A task that represents completion of the registered handlers.</returns>
    private static async Task RaiseEventAsync<T>(FieldInfo eventField, HubConnection connection, T argument)
    {
        var handler = (Func<T, Task>?)eventField.GetValue(connection)
            ?? throw new InvalidOperationException($"The {eventField.Name} event has no registered handler.");
        await handler(argument).ConfigureAwait(false);
    }
}
