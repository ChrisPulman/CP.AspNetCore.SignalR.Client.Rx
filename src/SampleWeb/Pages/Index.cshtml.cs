// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SignalRChat.Pages
{
    /// <summary>Index page model.</summary>
    /// <seealso cref="PageModel" />
    [SuppressMessage("Style", "SST1649:File name should match the first type", Justification = "Razor PageModel files use the .cshtml.cs convention.")]
    public class IndexModel : PageModel;
}
