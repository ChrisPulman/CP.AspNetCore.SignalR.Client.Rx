// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;

namespace SignalRChat.Hubs;

/// <summary>SignalR chat hub.</summary>
/// <seealso cref="Hub" />
public class ChatHub : Hub
{
    /// <summary>Sends the message.</summary>
    /// <param name="user">The user.</param>
    /// <param name="message">The message.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendMessage(string user, string message) => await Clients.All.SendAsync("ReceiveMessage", user, message);
}
