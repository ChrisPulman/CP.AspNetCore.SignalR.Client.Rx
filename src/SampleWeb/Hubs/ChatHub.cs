// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.SignalR;

namespace SignalRChat.Hubs;

/// <summary>
/// ChatHub.
/// </summary>
/// <seealso cref="Microsoft.AspNetCore.SignalR.Hub" />
public class ChatHub : Hub
{
    /// <summary>
    /// Sends the message.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="message">The message.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task SendMessage(string user, string message) => await Clients.All.SendAsync("ReceiveMessage", user, message);
}
