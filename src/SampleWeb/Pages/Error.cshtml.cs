// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SignalRChat.Pages
{
    /// <summary>
    /// ErrorModel.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.RazorPages.PageModel" />
    /// <remarks>
    /// Initializes a new instance of the <see cref="ErrorModel"/> class.
    /// </remarks>
    /// <param name="logger">The logger.</param>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable CS9113 // Parameter is unread.
    public class ErrorModel(ILogger<ErrorModel> logger) : PageModel
#pragma warning restore CS9113 // Parameter is unread.
#pragma warning restore SA1649 // File name should match first type name
    {
        /// <summary>
        /// Gets or sets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public string? RequestId { get; set; }

        /// <summary>
        /// Gets a value indicating whether [show request identifier].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show request identifier]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>
        /// Called when [get].
        /// </summary>
        public void OnGet() => RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }
}
