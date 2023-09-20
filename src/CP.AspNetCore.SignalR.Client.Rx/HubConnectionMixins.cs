// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Reactive;
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "Replicating base function.")]
    public static IObservable<Unit> StartAsync(this HubConnection connection)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromAsync(connection.StartAsync).Retry();
    }

    /// <summary>
    /// Starts a connection to the server.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None" />.</param>
    /// <returns>An Observable Unit.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "Replicating base function.")]
    public static IObservable<Unit> StartAsync(this HubConnection connection, CancellationToken cancellationToken = default)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromAsync(async () => await connection.StartAsync(cancellationToken)).Retry();
    }

    /// <summary>
    /// Stops a connection to the server.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>An Observable Unit.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Roslynator", "RCS1047:Non-asynchronous method name should not end with 'Async'.", Justification = "Replicating base function.")]
    public static IObservable<Unit> StopAsync(this HubConnection connection)
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
    public static IObservable<Unit> StopAsync(this HubConnection connection, CancellationToken cancellationToken = default)
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
    public static IObservable<T> Stream<T>(this HubConnection connection, string methodName, CancellationToken cancellationToken = default)
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
    public static IObservable<Exception?> Closed(this HubConnection connection)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromEvent<Func<Exception?, Task>, Exception>(x => connection.Closed += x, x => connection.Closed -= x);
    }

    /// <summary>
    /// Occurs when the <see cref="HubConnection"/> starts reconnecting after losing its underlying connection.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>A <see cref="IObservable{Exception}" />.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<Exception?> Reconnecting(this HubConnection connection)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromEvent<Func<Exception?, Task>, Exception>(x => connection.Reconnecting += x, x => connection.Reconnecting -= x);
    }

    /// <summary>
    /// Occurs when the <see cref="HubConnection"/> successfully reconnects after losing its underlying connection.
    /// </summary>
    /// <param name="connection">The connection.</param>
    /// <returns>A <see cref="IObservable{String}" />.</returns>
    /// <exception cref="System.ArgumentNullException">connection.</exception>
    public static IObservable<string?> Reconnected(this HubConnection connection)
    {
        if (connection == null)
        {
            throw new ArgumentNullException(nameof(connection));
        }

        return Observable.FromEvent<Func<string?, Task>, string>(x => connection.Reconnected += x, x => connection.Reconnected -= x);
    }
}
