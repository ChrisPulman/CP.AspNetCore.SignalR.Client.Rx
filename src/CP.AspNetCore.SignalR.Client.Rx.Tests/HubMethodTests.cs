// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.AspNetCore.SignalR.Client.Rx.Reactive.Tests;
#else
namespace CP.AspNetCore.SignalR.Client.Rx.Tests;
#endif

/// <summary>Tests strongly typed hub method descriptors.</summary>
public sealed class HubMethodTests
{
    /// <summary>Verifies that a descriptor exposes its method name and result type.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task DescriptorExposesNameAndResultType()
    {
        var method = new HubMethod<string>("Echo");

        await Assert.That(method.Name).IsEqualTo("Echo");
        await Assert.That(method.ResultType).IsEqualTo(typeof(string));
    }

    /// <summary>Verifies that a descriptor rejects a null method name.</summary>
    /// <returns>A task that represents the asynchronous test operation.</returns>
    [Test]
    public async Task DescriptorRejectsNullName() =>
        await Assert.That(static () => new HubMethod<string>(null!)).Throws<ArgumentNullException>();
}
