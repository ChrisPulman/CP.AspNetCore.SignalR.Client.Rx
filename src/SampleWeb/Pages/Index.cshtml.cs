// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SignalRChat.Pages
{
    /// <summary>
    /// IndexModel.
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.RazorPages.PageModel" />
    /// <remarks>
    /// Initializes a new instance of the <see cref="IndexModel"/> class.
    /// </remarks>
    /// <param name="logger">The logger.</param>
#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable CS9113 // Parameter is unread.
    public class IndexModel(ILogger<IndexModel> logger) : PageModel
#pragma warning restore CS9113 // Parameter is unread.
#pragma warning restore SA1649 // File name should match first type name
    {
        /// <summary>
        /// Called when [get].
        /// </summary>
        public void OnGet()
        {
        }
    }
}
