// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.AspNetCore.SignalR.Client.Rx.Reactive;
#else
namespace CP.AspNetCore.SignalR.Client.Rx;
#endif

/// <summary>HubConnection Mixins.</summary>
public static class HubConnectionMixins
{
    /// <summary>Provides observable wrappers for a hub connection.</summary>
    /// <param name="connection">The hub connection.</param>
    extension(HubConnection connection)
    {
        /// <summary>Starts a connection to the server.</summary>
        /// <returns>An Observable that completes when the connection has started.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<RxVoid> StartObservable()
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return Observable.FromAsync(() => ToRxVoidAsync(connection.StartAsync())).OnErrorRetry();
        }

        /// <summary>Starts a connection to the server.</summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
        /// <returns>An Observable that completes when the connection has started.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<RxVoid> StartObservable(CancellationToken cancellationToken)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return Observable.FromAsync(() => ToRxVoidAsync(connection.StartAsync(cancellationToken))).OnErrorRetry();
        }

        /// <summary>Starts a connection to the server with a retry count.</summary>
        /// <param name="retryCount">Number of retry attempts on failure. Default infinite (<c>null</c>).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An Observable that completes when the connection has started.</returns>
        public IObservable<RxVoid> StartObservable(int? retryCount, CancellationToken cancellationToken)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            var source = Observable.FromAsync(() => ToRxVoidAsync(connection.StartAsync(cancellationToken)));
            return ApplyRetry(source, retryCount);
        }

        /// <summary>Ensure a connection is started. If already connected, completes immediately.</summary>
        /// <returns>An observable that completes when the connection has started.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<RxVoid> EnsureStarted() => connection.EnsureStarted(default);

        /// <summary>Ensure a connection is started. If already connected, completes immediately.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An observable that completes when the connection has started.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<RxVoid> EnsureStarted(CancellationToken cancellationToken)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return connection.State == HubConnectionState.Disconnected
                ? connection.StartObservable(cancellationToken)
                : Observable.Return(RxVoid.Default);
        }

        /// <summary>Stops a connection to the server.</summary>
        /// <returns>An Observable that completes when the connection has stopped.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<RxVoid> StopObservable()
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return Observable.FromAsync(() => ToRxVoidAsync(connection.StopAsync())).OnErrorRetry();
        }

        /// <summary>Stops a connection to the server.</summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
        /// <returns>An Observable that completes when the connection has stopped.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<RxVoid> StopObservable(CancellationToken cancellationToken)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return Observable.FromAsync(() => ToRxVoidAsync(connection.StopAsync(cancellationToken))).OnErrorRetry();
        }

        /// <summary>Stops a connection to the server with a retry count.</summary>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An Observable that completes when the connection has stopped.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<RxVoid> StopObservable(int? retryCount, CancellationToken cancellationToken)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            var source = Observable.FromAsync(() => ToRxVoidAsync(connection.StopAsync(cancellationToken)));
            return ApplyRetry(source, retryCount);
        }

        /// <summary>Invokes a streaming hub method on the server using the specified method name and return type.</summary>
        /// <typeparam name="T">The return type of the streaming server method.</typeparam>
        /// <param name="method">The strongly typed hub method.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
        /// <returns>
        /// A <see cref="IObservable{T}" /> that represents the stream.
        /// </returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<T> StreamObservable<T>(HubMethod<T> method, CancellationToken cancellationToken)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            // Use a CTS linked to the subscription so we can cancel the streaming when disposed.
            return Observable.Create<T>(async (observer, subscriptionToken) =>
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, subscriptionToken);

                try
                {
                    await foreach (var item in connection.StreamAsync<T>(method.Name, cts.Token).WithCancellation(cts.Token).ConfigureAwait(false))
                    {
                        observer.OnNext(item);
                    }

                    observer.OnCompleted();
                }
                catch (OperationCanceledException)
                {
                    observer.OnCompleted();
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>Invokes a streaming hub method on the server using the specified method name, return type and arguments.</summary>
        /// <typeparam name="T">The return type of the streaming server method.</typeparam>
        /// <param name="method">The strongly typed hub method.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="args">Arguments for the hub method.</param>
        /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
        public IObservable<T> StreamObservable<T>(HubMethod<T> method, CancellationToken cancellationToken, params object?[] args)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return Observable.Create<T>(async (observer, subscriptionToken) =>
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, subscriptionToken);

                try
                {
                    var reader = await connection.StreamAsChannelCoreAsync<T>(method.Name, args, cts.Token).ConfigureAwait(false);
                    while (await reader.WaitToReadAsync(cts.Token).ConfigureAwait(false))
                    {
                        while (reader.TryRead(out var item))
                        {
                            observer.OnNext(item);
                        }
                    }

                    observer.OnCompleted();
                }
                catch (OperationCanceledException)
                {
                    observer.OnCompleted();
                }
                catch (Exception error)
                {
                    observer.OnError(error);
                }

                return Disposable.Empty;
            });
        }

        /// <summary>Invokes a hub method on the server using the specified method name and arguments and returns a result as an observable.</summary>
        /// <typeparam name="T">The return type of the streaming server method.</typeparam>
        /// <param name="method">The strongly typed hub method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<T> InvokeObservable<T>(HubMethod<T> method, CancellationToken cancellationToken, params object?[] args)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return Observable.FromAsync(() => connection.InvokeCoreAsync<T>(method.Name, args, cancellationToken));
        }

        /// <summary>
        /// Invokes a hub method on the server using the specified method name and arguments and returns completion as an observable.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<RxVoid> InvokeObservable(string methodName, CancellationToken cancellationToken, params object?[] args)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return Observable.FromAsync(() => ToRxVoidAsync(connection.InvokeCoreAsync(methodName, args, cancellationToken)));
        }

        /// <summary>Sends a hub method on the server using the specified method name and arguments and returns completion as an observable.</summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<RxVoid> SendObservable(string methodName, CancellationToken cancellationToken, params object?[] args)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return Observable.FromAsync(() => ToRxVoidAsync(connection.SendCoreAsync(methodName, args, cancellationToken)));
        }

        /// <summary>
        /// Occurs when the connection is closed. The connection could be closed due to an error or due to either the server or client intentionally
        /// closing the connection without error.
        /// </summary>
        /// <returns>A <see cref="IObservable{Exception}" />.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<Exception?> HasClosed()
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return Observable.Create<Exception?>(observer =>
            {
                Task ClosedHandler(Exception? error)
                {
                    observer.OnNext(error);
                    return Task.CompletedTask;
                }

                connection.Closed += ClosedHandler;

                return Disposable.Create<(HubConnection connection, Func<Exception?, Task> handler)>(
                    (connection, ClosedHandler),
                    static state => state.connection.Closed -= state.handler);
            });
        }

        /// <summary>Occurs when the <see cref="HubConnection"/> starts reconnecting after losing its underlying connection.</summary>
        /// <returns>The <see cref="Exception"/> that occurred will be passed in as the sole argument to this handler.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<Exception?> IsReconnecting()
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return Observable.Create<Exception?>(observer =>
            {
                Task ReconnectingHandler(Exception? error)
                {
                    observer.OnNext(error);
                    return Task.CompletedTask;
                }

                connection.Reconnecting += ReconnectingHandler;

                return Disposable.Create<(HubConnection connection, Func<Exception?, Task> handler)>(
                    (connection, ReconnectingHandler),
                    static state => state.connection.Reconnecting -= state.handler);
            });
        }

        /// <summary>Occurs when the <see cref="HubConnection"/> successfully reconnects after losing its underlying connection.</summary>
        /// <returns>Return value will be the <see cref="HubConnection"/>'s new ConnectionId or null if negotiation was skipped.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<string?> HasReconnected()
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return Observable.Create<string?>(observer =>
            {
                Task ReconnectedHandler(string? connectionId)
                {
                    observer.OnNext(connectionId);
                    return Task.CompletedTask;
                }

                connection.Reconnected += ReconnectedHandler;

                return Disposable.Create<(HubConnection connection, Func<string?, Task> handler)>(
                    (connection, ReconnectedHandler),
                    static state => state.connection.Reconnected -= state.handler);
            });
        }

        /// <summary>Observe state changes of the HubConnection starting with the current state.</summary>
        /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<HubConnectionState> StateChanges()
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return Observable.Merge(
                    connection.HasClosed().Select(_ => connection.State),
                    connection.IsReconnecting().Select(_ => connection.State),
                    connection.HasReconnected().Select(_ => connection.State))
                .StartWith(connection.State)
                .DistinctUntilChanged();
        }

        /// <summary>Wait until the connection reaches the specified state, then complete.</summary>
        /// <param name="desiredState">State of the desired.</param>
        /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<HubConnectionState> WaitForState(HubConnectionState desiredState)
        {
            _ = connection ?? throw new ArgumentNullException(nameof(connection));

            return connection.StateChanges().Where(s => s == desiredState).Take(1);
        }
    }

    /// <summary>Provides start helpers for observable hub connection sources.</summary>
    /// <param name="connection">The observable hub connection source.</param>
    extension(IObservable<HubConnection> connection)
    {
        /// <summary>Starts the specified connection.</summary>
        /// <returns>Observable HubConnection.</returns>
        public IObservable<HubConnection> Start() =>
            connection.SelectMany(x => x.StartObservable().Select(_ => x));

        /// <summary>Starts the specified connection.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// Observable HubConnection.
        /// </returns>
        public IObservable<HubConnection> Start(CancellationToken cancellationToken) =>
            connection.SelectMany(x => x.StartObservable(cancellationToken).Select(_ => x));

        /// <summary>Starts the specified connection with a retry count.</summary>
        /// <param name="retryCount">Number of retry attempts on failure. Default infinite (<c>null</c>).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Observable HubConnection.</returns>
        public IObservable<HubConnection> Start(int? retryCount, CancellationToken cancellationToken) =>
            connection.SelectMany(x => x.StartObservable(retryCount, cancellationToken).Select(_ => x));
    }

    /// <summary>Provides start triggers for arbitrary observable sources.</summary>
    /// <typeparam name="T">The source value type.</typeparam>
    /// <param name="ignore">The observable source that triggers connection start.</param>
    extension<T>(IObservable<T> ignore)
    {
        /// <summary>Starts the specified connection.</summary>
        /// <param name="connection">The connection.</param>
        /// <returns>Observable HubConnection.</returns>
        public IObservable<HubConnection> Start(HubConnection connection) =>
            ignore.Select(_ => connection).SelectMany(x => x.StartObservable().Select(_ => x));

        /// <summary>Starts the specified connection.</summary>
        /// <param name="connection">The connection.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>
        /// Observable HubConnection.
        /// </returns>
        public IObservable<HubConnection> Start(HubConnection connection, CancellationToken cancellationToken) =>
            ignore.Select(_ => connection).SelectMany(x => x.StartObservable(cancellationToken).Select(_ => x));

        /// <summary>Starts the specified connection with a retry count.</summary>
        /// <param name="connection">The connection.</param>
        /// <param name="retryCount">Number of retry attempts on failure. Default infinite (<c>null</c>).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Observable HubConnection.</returns>
        public IObservable<HubConnection> Start(HubConnection connection, int? retryCount, CancellationToken cancellationToken) =>
            ignore.Select(_ => connection).SelectMany(x => x.StartObservable(retryCount, cancellationToken).Select(_ => x));
    }

    /// <summary>Applies classic Rx retry-count semantics to a source.</summary>
    /// <param name="source">The source observable.</param>
    /// <param name="retryCount">Total permitted subscriptions, or <see langword="null"/> for unbounded retry.</param>
    /// <returns>The retried observable.</returns>
    private static IObservable<RxVoid> ApplyRetry(IObservable<RxVoid> source, int? retryCount)
    {
        if (!retryCount.HasValue)
        {
            return source.OnErrorRetry();
        }

        return retryCount.Value == 0
            ? Observable.Empty<RxVoid>()
            : source.Retry(retryCount.Value - 1);
    }

    /// <summary>Converts a non-generic task into a reactive void task.</summary>
    /// <param name="task">The task to observe.</param>
    /// <returns>A task that emits the reactive void value after completion.</returns>
    private static async Task<RxVoid> ToRxVoidAsync(Task task)
    {
        await task.ConfigureAwait(false);
        return RxVoid.Default;
    }
}
