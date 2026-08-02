// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace SignalRChat.Hubs;

/// <summary>SignalR chat hub.</summary>
/// <seealso cref="Hub" />
public class ChatHub : Hub
{
    /// <summary>Sends the message.</summary>
    /// <param name="user">The user.</param>
    /// <param name="message">The message.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task SendMessage(string user, string message) => Clients.All.SendAsync("ReceiveMessage", user, message);
}
