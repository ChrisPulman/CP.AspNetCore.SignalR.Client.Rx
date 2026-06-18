// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.AspNetCore.SignalR.Client;

namespace CP.AspNetCore.SignalR.Client.Rx;

/// <summary>Builds observable SignalR hub connections.</summary>
public static class HubBuilder
{
    /// <summary>Creates a HubConnection.</summary>
    /// <param name="hubConnectionBuilder">The hub connection builder.</param>
    /// <returns>A HubConnection.</returns>
    public static IObservable<(HubConnection hubConnection, CompositeDisposable disposables)> Create(Func<HubConnectionBuilder, IHubConnectionBuilder> hubConnectionBuilder)
    {
        if (hubConnectionBuilder is null)
        {
            throw new ArgumentNullException(nameof(hubConnectionBuilder));
        }

        return Observable.Create<(HubConnection hubConnection, CompositeDisposable disposables)>(observer =>
        {
            var disposables = new CompositeDisposable();
            var connection = hubConnectionBuilder(new HubConnectionBuilder()).Build();
            observer.OnNext((connection, disposables));
            disposables.Add(Disposable.Create(async () => await DisposeConnectionAsync(connection)));
            return disposables;
        });
    }

    /// <summary>Disposes the hub connection asynchronously.</summary>
    /// <param name="connection">The hub connection.</param>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    private static async Task DisposeConnectionAsync(HubConnection connection)
    {
        if (connection is null)
        {
            return;
        }

        await connection.DisposeAsync();
    }
}
