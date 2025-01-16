// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
    /// <returns>An Observable Unit.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<Unit> StartObservable(this HubConnection connection)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromAsync(async () => await connection.StartAsync()).Retry();
    }

    /// <summary>
    /// Starts a connection to the server.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
    /// <returns>An Observable Unit.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<Unit> StartObservable(this HubConnection connection, CancellationToken cancellationToken = default)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromAsync(async () => await connection.StartAsync(cancellationToken)).Retry();
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
    /// Stops a connection to the server.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>An Observable Unit.</returns>
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
    /// <returns>An Observable Unit.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "Replicating base function.")]
    public static IObservable<Unit> StopObservable(this HubConnection connection, CancellationToken cancellationToken = default)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromAsync(async () => await connection.StopAsync(cancellationToken)).Retry();
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

        return Observable.Create<T>(async observer =>
        {
            await foreach (var x in connection.StreamAsync<T>(methodName, cancellationToken))
            {
                observer.OnNext(x);
            }

            observer.OnCompleted();
        });
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
}
