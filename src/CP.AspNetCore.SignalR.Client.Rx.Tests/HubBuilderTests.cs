// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.AspNetCore.SignalR.Client.Rx.Reactive.Tests;
#else
namespace CP.AspNetCore.SignalR.Client.Rx.Tests;
#endif

/// <summary>Tests observable hub connection creation.</summary>
public sealed class HubBuilderTests
{
    /// <summary>Verifies that a missing builder delegate is rejected.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task CreateThrowsWhenBuilderDelegateIsNull()
    {
        await Assert.That(static () => HubBuilder.Create(null!)).Throws<ArgumentNullException>();
    }

    /// <summary>Verifies that the created observable emits a connection and owns the disposable scope.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task CreateEmitsConnectionAndDisposableScope()
    {
        (HubConnection HubConnection, CompositeDisposable Disposables)? created = null;

        using var subscription = HubBuilder.Create(static builder => builder.WithUrl("http://localhost/testHub"))
            .Subscribe(value => created = value);

        await Assert.That(created).IsNotNull();
        await Assert.That(created!.Value.HubConnection.State).IsEqualTo(HubConnectionState.Disconnected);
        await Assert.That(created.Value.Disposables.IsDisposed).IsFalse();

        subscription.Dispose();

        await Assert.That(created.Value.Disposables.IsDisposed).IsTrue();
    }
}
