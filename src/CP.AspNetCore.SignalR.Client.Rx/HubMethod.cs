// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#if REACTIVE_SHIM
namespace CP.AspNetCore.SignalR.Client.Rx.Reactive;
#else
namespace CP.AspNetCore.SignalR.Client.Rx;
#endif

/// <summary>Identifies a strongly typed SignalR hub method.</summary>
/// <typeparam name="TResult">The result or callback tuple type.</typeparam>
/// <param name="name">The hub method name.</param>
public sealed class HubMethod<TResult>(string name)
{
    /// <summary>Gets the hub method name.</summary>
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    /// <summary>Gets the result type represented by this method descriptor.</summary>
    public Type ResultType => typeof(TResult);
}
