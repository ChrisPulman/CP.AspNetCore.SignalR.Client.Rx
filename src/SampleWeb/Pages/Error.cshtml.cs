// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SignalRChat.Pages
{
    /// <summary>Error page model.</summary>
    /// <seealso cref="PageModel" />
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    [SuppressMessage("Style", "SST1649:File name should match the first type", Justification = "Razor PageModel files use the .cshtml.cs convention.")]
    public class ErrorModel : PageModel
    {
        /// <summary>Gets or sets the request identifier.</summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public string? RequestId { get; set; }

        /// <summary>Gets a value indicating whether [show request identifier].</summary>
        /// <value>
        ///   <c>true</c> if [show request identifier]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        /// <summary>Called when [get].</summary>
        public void OnGet() => RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }
}
