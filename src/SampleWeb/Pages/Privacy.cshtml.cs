// Copyright (c) 2023-2026 Chris Pulman and Contributors. All rights reserved.
// Chris Pulman and Contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SignalRChat.Pages
{
    /// <summary>Privacy page model.</summary>
    /// <seealso cref="PageModel" />
    [SuppressMessage("Style", "SST1649:File name should match the first type", Justification = "Razor PageModel files use the .cshtml.cs convention.")]
    public class PrivacyModel : PageModel;
}
