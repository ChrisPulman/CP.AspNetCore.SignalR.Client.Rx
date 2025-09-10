// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Channels;
using Microsoft.AspNetCore.SignalR.Client;

namespace CP.AspNetCore.SignalR.Client.Rx;

/// <summary>
/// HubConnection Mixins.
/// </summary>
public static class HubConnectionMixins
{
    /// <summary>
    /// Starts a connection to the server.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>An Observable that completes when the connection has started.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<Unit> StartObservable(this HubConnection connection)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromAsync(() => connection.StartAsync()).Retry();
    }

    /// <summary>
    /// Starts a connection to the server.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
    /// <returns>An Observable that completes when the connection has started.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<Unit> StartObservable(this HubConnection connection, CancellationToken cancellationToken = default)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromAsync(() => connection.StartAsync(cancellationToken)).Retry();
    }

    /// <summary>
    /// Starts a connection to the server with a retry count.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="retryCount">Number of retry attempts on failure. Default infinite (<c>null</c>).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An Observable that completes when the connection has started.</returns>
    public static IObservable<Unit> StartObservable(this HubConnection connection, int? retryCount, CancellationToken cancellationToken = default)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        var src = Observable.FromAsync(() => connection.StartAsync(cancellationToken));
        return retryCount.HasValue ? src.Retry(retryCount.Value) : src.Retry();
    }

    /// <summary>
    /// Starts the specified connection.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>Observable HubConnection.</returns>
    public static IObservable<HubConnection> Start(this IObservable<HubConnection> connection) =>
        connection.SelectMany(x => x.StartObservable().Select(_ => x));

    /// <summary>
    /// Starts the specified connection.
    /// </summary>
    /// <typeparam name="T">The type of the base.</typeparam>
    /// <param name="ignore">The ignore.</param>
    /// <param name="connection">The connection.</param>
    /// <returns>Observable HubConnection.</returns>
    public static IObservable<HubConnection> Start<T>(this IObservable<T> ignore, HubConnection connection) =>
        ignore.Select(_ => connection).SelectMany(x => x.StartObservable().Select(_ => x));

    /// <summary>
    /// Starts the specified connection.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// Observable HubConnection.
    /// </returns>
    public static IObservable<HubConnection> Start(this IObservable<HubConnection> connection, CancellationToken cancellationToken = default) =>
        connection.SelectMany(x => x.StartObservable(cancellationToken).Select(_ => x));

    /// <summary>
    /// Starts the specified connection with a retry count.
    /// </summary>
    /// <param name="connection">The connection source.</param>
    /// <param name="retryCount">Number of retry attempts on failure. Default infinite (<c>null</c>).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Observable HubConnection.</returns>
    public static IObservable<HubConnection> Start(this IObservable<HubConnection> connection, int? retryCount, CancellationToken cancellationToken = default) =>
        connection.SelectMany(x => x.StartObservable(retryCount, cancellationToken).Select(_ => x));

    /// <summary>
    /// Starts the specified connection.
    /// </summary>
    /// <typeparam name="T">The type of the base.</typeparam>
    /// <param name="ignore">The ignore.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    /// Observable HubConnection.
    /// </returns>
    public static IObservable<HubConnection> Start<T>(this IObservable<T> ignore, HubConnection connection, CancellationToken cancellationToken = default) =>
        ignore.Select(_ => connection).SelectMany(x => x.StartObservable(cancellationToken).Select(_ => x));

    /// <summary>
    /// Starts the specified connection with a retry count.
    /// </summary>
    /// <typeparam name="T">Any.</typeparam>
    /// <param name="ignore">The source to trigger starting.</param>
    /// <param name="connection">The connection.</param>
    /// <param name="retryCount">Number of retry attempts on failure. Default infinite (<c>null</c>).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Observable HubConnection.</returns>
    public static IObservable<HubConnection> Start<T>(this IObservable<T> ignore, HubConnection connection, int? retryCount, CancellationToken cancellationToken = default) =>
        ignore.Select(_ => connection).SelectMany(x => x.StartObservable(retryCount, cancellationToken).Select(_ => x));

    /// <summary>
    /// Ensure a connection is started. If already connected, completes immediately.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An Observable that completes when the connection has stopped.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<Unit> EnsureStarted(this HubConnection connection, CancellationToken cancellationToken = default)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return connection.State == HubConnectionState.Disconnected
            ? connection.StartObservable(cancellationToken)
            : Observable.Return(Unit.Default);
    }

    /// <summary>
    /// Stops a connection to the server.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>An Observable that completes when the connection has stopped.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<Unit> StopObservable(this HubConnection connection)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromAsync(connection.StopAsync).Retry();
    }

    /// <summary>
    /// Stops a connection to the server.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
    /// <returns>An Observable that completes when the connection has stopped.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "Replicating base function.")]
    public static IObservable<Unit> StopObservable(this HubConnection connection, CancellationToken cancellationToken = default)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromAsync(() => connection.StopAsync(cancellationToken)).Retry();
    }

    /// <summary>
    /// Stops a connection to the server with a retry count.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An Observable that completes when the connection has stopped.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<Unit> StopObservable(this HubConnection connection, int? retryCount, CancellationToken cancellationToken = default)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        var src = Observable.FromAsync(() => connection.StopAsync(cancellationToken));
        return retryCount.HasValue ? src.Retry(retryCount.Value) : src.Retry();
    }

    /// <summary>
    /// Invokes a streaming hub method on the server using the specified method name and return type.
    /// </summary>
    /// <typeparam name="T">The return type of the streaming server method.</typeparam>
    /// <param name="connection">The connection.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
    /// <returns>
    /// A <see cref="IObservable{T}" /> that represents the stream.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<T> StreamObservable<T>(this HubConnection connection, string methodName, CancellationToken cancellationToken = default)
    {
        if (connection == null)
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

    /// <summary>
    /// Invokes a streaming hub method on the server using the specified method name, return type and arguments.
    /// </summary>
    /// <typeparam name="T">The return type of the streaming server method.</typeparam>
    /// <param name="connection">The connection.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="args">Arguments for the hub method.</param>
    /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
    public static IObservable<T> StreamObservable<T>(this HubConnection connection, string methodName, CancellationToken cancellationToken = default, params object?[] args)
    {
        if (connection == null)
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
                    var reader = await connection.StreamAsChannelAsync<T>(methodName, args, cts.Token).ConfigureAwait(false);
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

    /// <summary>
    /// Invokes a hub method on the server using the specified method name and arguments and returns a result as an observable.
    /// </summary>
    /// <typeparam name="T">The return type of the streaming server method.</typeparam>
    /// <param name="connection">The connection.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<T> InvokeObservable<T>(this HubConnection connection, string methodName, CancellationToken cancellationToken = default, params object?[] args)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromAsync(() => connection.InvokeAsync<T>(methodName, args, cancellationToken));
    }

    /// <summary>
    /// Invokes a hub method on the server using the specified method name and arguments and returns completion as an observable.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<Unit> InvokeObservable(this HubConnection connection, string methodName, CancellationToken cancellationToken = default, params object?[] args)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromAsync(() => connection.InvokeAsync(methodName, args, cancellationToken));
    }

    /// <summary>
    /// Sends a hub method on the server using the specified method name and arguments and returns completion as an observable.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="methodName">Name of the method.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <param name="args">The arguments.</param>
    /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<Unit> SendObservable(this HubConnection connection, string methodName, CancellationToken cancellationToken = default, params object?[] args)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromAsync(() => connection.SendAsync(methodName, args, cancellationToken));
    }

    /// <summary>
    /// Occurs when the connection is closed. The connection could be closed due to an error or due to either the server or client intentionally
    /// closing the connection without error.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>A <see cref="IObservable{Exception}" />.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<Exception?> HasClosed(this HubConnection connection)
    {
        if (connection == null)
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

    /// <summary>
    /// Occurs when the <see cref="HubConnection"/> starts reconnecting after losing its underlying connection.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>The <see cref="Exception"/> that occurred will be passed in as the sole argument to this handler.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<Exception?> IsReconnecting(this HubConnection connection)
    {
        if (connection == null)
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

    /// <summary>
    /// Occurs when the <see cref="HubConnection"/> successfully reconnects after losing its underlying connection.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>Return value will be the <see cref="HubConnection"/>'s new ConnectionId or null if negotiation was skipped.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<string?> HasReconnected(this HubConnection connection)
    {
        if (connection == null)
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

    /// <summary>
    /// Observe state changes of the HubConnection starting with the current state.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<HubConnectionState> StateChanges(this HubConnection connection)
    {
        if (connection == null)
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

    /// <summary>
    /// Wait until the connection reaches the specified state, then complete.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="desiredState">State of the desired.</param>
    /// <returns>A <see cref="IObservable{T}"/> that represents the stream.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<HubConnectionState> WaitForState(this HubConnection connection, HubConnectionState desiredState)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return connection.StateChanges().Where(s => s == desiredState).Take(1);
    }
}
