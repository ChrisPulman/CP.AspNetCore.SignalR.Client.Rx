// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR.Client;

namespace CP.AspNetCore.SignalR.Client.Rx;

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
        public IObservable<Unit> StartObservable()
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return Observable.FromAsync(() => connection.StartAsync()).Retry();
        }

        /// <summary>Starts a connection to the server.</summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
        /// <returns>An Observable that completes when the connection has started.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<Unit> StartObservable(CancellationToken cancellationToken = default)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return Observable.FromAsync(() => connection.StartAsync(cancellationToken)).Retry();
        }

        /// <summary>Starts a connection to the server with a retry count.</summary>
        /// <param name="retryCount">Number of retry attempts on failure. Default infinite (<c>null</c>).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An Observable that completes when the connection has started.</returns>
        public IObservable<Unit> StartObservable(int? retryCount, CancellationToken cancellationToken = default)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            var src = Observable.FromAsync(() => connection.StartAsync(cancellationToken));
            return retryCount.HasValue ? src.Retry(retryCount.Value) : src.Retry();
        }

        /// <summary>Ensure a connection is started. If already connected, completes immediately.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An Observable that completes when the connection has stopped.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<Unit> EnsureStarted(CancellationToken cancellationToken = default)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return connection.State == HubConnectionState.Disconnected
                ? connection.StartObservable(cancellationToken)
                : Observable.Return(Unit.Default);
        }

        /// <summary>Stops a connection to the server.</summary>
        /// <returns>An Observable that completes when the connection has stopped.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<Unit> StopObservable()
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return Observable.FromAsync(connection.StopAsync).Retry();
        }

        /// <summary>Stops a connection to the server.</summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
        /// <returns>An Observable that completes when the connection has stopped.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "Replicating base function.")]
        public IObservable<Unit> StopObservable(CancellationToken cancellationToken = default)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return Observable.FromAsync(() => connection.StopAsync(cancellationToken)).Retry();
        }

        /// <summary>Stops a connection to the server with a retry count.</summary>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An Observable that completes when the connection has stopped.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<Unit> StopObservable(int? retryCount, CancellationToken cancellationToken = default)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            var src = Observable.FromAsync(() => connection.StopAsync(cancellationToken));
            return retryCount.HasValue ? src.Retry(retryCount.Value) : src.Retry();
        }

        /// <summary>Invokes a streaming hub method on the server using the specified method name and return type.</summary>
        /// <typeparam name="T">The return type of the streaming server method.</typeparam>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
        /// <returns>
        /// A <see cref="IObservable{T}" /> that represents the stream.
        /// </returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<T> StreamObservable<T>(string methodName, CancellationToken cancellationToken = default)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            // Use a CTS linked to the subscription so we can cancel the streaming when disposed.
            return Observable.Create<T>(observer =>
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                _ = Task.Run(
                    async () =>
                {
                    try
                    {
                        await foreach (var x in connection.StreamAsync<T>(methodName, cts.Token).WithCancellation(cts.Token).ConfigureAwait(false))
                        {
                            observer.OnNext(x);
                        }

                        observer.OnCompleted();
                    }
                    catch (OperationCanceledException)
                    {
                        // Swallow cancellation; treat as completion.
                        observer.OnCompleted();
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                    cts.Token);

                return Disposable.Create(() => cts.Cancel());
            });
        }

        /// <summary>Invokes a streaming hub method on the server using the specified method name, return type and arguments.</summary>
        /// <typeparam name="T">The return type of the streaming server method.</typeparam>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="args">Arguments for the hub method.</param>
        /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
        public IObservable<T> StreamObservable<T>(string methodName, CancellationToken cancellationToken = default, params object?[] args)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return Observable.Create<T>(observer =>
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                _ = Task.Run(
                    async () =>
                {
                    try
                    {
                        // Prefer ChannelReader for efficiency, then drain it.
                        var reader = await connection.StreamAsChannelCoreAsync<T>(methodName, args, cts.Token).ConfigureAwait(false);
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
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                    cts.Token);

                return Disposable.Create(() => cts.Cancel());
            });
        }

        /// <summary>Invokes a hub method on the server using the specified method name and arguments and returns a result as an observable.</summary>
        /// <typeparam name="T">The return type of the streaming server method.</typeparam>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<T> InvokeObservable<T>(string methodName, CancellationToken cancellationToken = default, params object?[] args)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return Observable.FromAsync(() => connection.InvokeCoreAsync<T>(methodName, args, cancellationToken));
        }

        /// <summary>
        /// Invokes a hub method on the server using the specified method name and arguments and returns completion as an observable.
        /// </summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<Unit> InvokeObservable(string methodName, CancellationToken cancellationToken = default, params object?[] args)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return Observable.FromAsync(() => connection.InvokeCoreAsync(methodName, args, cancellationToken));
        }

        /// <summary>Sends a hub method on the server using the specified method name and arguments and returns completion as an observable.</summary>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<Unit> SendObservable(string methodName, CancellationToken cancellationToken = default, params object?[] args)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return Observable.FromAsync(() => connection.SendCoreAsync(methodName, args, cancellationToken));
        }

        /// <summary>
        /// Occurs when the connection is closed. The connection could be closed due to an error or due to either the server or client intentionally
        /// closing the connection without error.
        /// </summary>
        /// <returns>A <see cref="IObservable{Exception}" />.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<Exception?> HasClosed()
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return Observable.Create<Exception?>(observer =>
            {
                Task ClosedHandler(Exception? error)
                {
                    observer.OnNext(error);
                    return Task.CompletedTask;
                }

                connection.Closed += ClosedHandler;

                return Disposable.Create(() => connection.Closed -= ClosedHandler);
            });
        }

        /// <summary>Occurs when the <see cref="HubConnection"/> starts reconnecting after losing its underlying connection.</summary>
        /// <returns>The <see cref="Exception"/> that occurred will be passed in as the sole argument to this handler.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<Exception?> IsReconnecting()
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return Observable.Create<Exception?>(observer =>
            {
                Task ReconnectingHandler(Exception? error)
                {
                    observer.OnNext(error);
                    return Task.CompletedTask;
                }

                connection.Reconnecting += ReconnectingHandler;

                return Disposable.Create(() => connection.Reconnecting -= ReconnectingHandler);
            });
        }

        /// <summary>Occurs when the <see cref="HubConnection"/> successfully reconnects after losing its underlying connection.</summary>
        /// <returns>Return value will be the <see cref="HubConnection"/>'s new ConnectionId or null if negotiation was skipped.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<string?> HasReconnected()
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return Observable.Create<string?>(observer =>
            {
                Task ReconnectedHandler(string? connectionId)
                {
                    observer.OnNext(connectionId);
                    return Task.CompletedTask;
                }

                connection.Reconnected += ReconnectedHandler;

                return Disposable.Create(() => connection.Reconnected -= ReconnectedHandler);
            });
        }

        /// <summary>Observe state changes of the HubConnection starting with the current state.</summary>
        /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
        /// <exception cref="ArgumentNullException">connection.</exception>
        public IObservable<HubConnectionState> StateChanges()
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

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
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

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
        public IObservable<HubConnection> Start(CancellationToken cancellationToken = default) =>
            connection.SelectMany(x => x.StartObservable(cancellationToken).Select(_ => x));

        /// <summary>Starts the specified connection with a retry count.</summary>
        /// <param name="retryCount">Number of retry attempts on failure. Default infinite (<c>null</c>).</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Observable HubConnection.</returns>
        public IObservable<HubConnection> Start(int? retryCount, CancellationToken cancellationToken = default) =>
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
        public IObservable<HubConnection> Start(HubConnection connection, CancellationToken cancellationToken = default) =>
            ignore.Select(_ => connection).SelectMany(x => x.StartObservable(cancellationToken).Select(_ => x));

        /// <summary>Starts the specified connection with a retry count.</summary>
        /// <param name="connection">The connection.</param>
        /// <param name="retryCount">Number of retry attempts on failure. Default infinite (<c>null</c>).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Observable HubConnection.</returns>
        public IObservable<HubConnection> Start(HubConnection connection, int? retryCount, CancellationToken cancellationToken = default) =>
            ignore.Select(_ => connection).SelectMany(x => x.StartObservable(retryCount, cancellationToken).Select(_ => x));
    }
}
