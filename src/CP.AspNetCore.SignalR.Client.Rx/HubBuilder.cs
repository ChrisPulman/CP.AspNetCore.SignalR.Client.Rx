// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.AspNetCore.SignalR.Client.Rx.Reactive;
#else
namespace CP.AspNetCore.SignalR.Client.Rx;
#endif

/// <summary>Builds observable SignalR hub connections.</summary>
public static class HubBuilder
{
    /// <summary>Creates a HubConnection.</summary>
    /// <param name="hubConnectionBuilder">The hub connection builder.</param>
    /// <returns>A HubConnection.</returns>
    public static IObservable<(HubConnection hubConnection, CompositeDisposable disposables)> Create(Func<HubConnectionBuilder, IHubConnectionBuilder> hubConnectionBuilder)
    {
        _ = hubConnectionBuilder ?? throw new ArgumentNullException(nameof(hubConnectionBuilder));

        return Observable.CreateSafe<(HubConnection hubConnection, CompositeDisposable disposables)>(observer =>
        {
            var disposables = new CompositeDisposable();
            var connection = hubConnectionBuilder(new()).Build();
            observer.OnNext((connection, disposables));
            disposables.Add(Disposable.Create(connection, DisposeConnection));
            return disposables;
        });
    }

    /// <summary>Disposes the hub connection.</summary>
    /// <param name="connection">The hub connection.</param>
    private static void DisposeConnection(HubConnection connection) =>
        ObserveDisposal(connection.DisposeAsync());

    /// <summary>Observes a pending asynchronous disposal operation.</summary>
    /// <param name="disposal">The disposal operation.</param>
    private static void ObserveDisposal(ValueTask disposal) =>
        new DisposeContinuation(disposal.GetAwaiter()).Register();

    /// <summary>Observes asynchronous connection disposal without blocking the disposing thread.</summary>
    /// <param name="awaiter">The disposal awaiter.</param>
    private sealed class DisposeContinuation(ValueTaskAwaiter awaiter)
    {
        /// <summary>Registers the completion callback.</summary>
        public void Register() => awaiter.OnCompleted(Complete);

        /// <summary>Observes the asynchronous disposal result.</summary>
        private void Complete() => awaiter.GetResult();
    }
}
