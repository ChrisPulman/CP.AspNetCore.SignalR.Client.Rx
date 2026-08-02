// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace SignalRChat;

/// <summary>Hosts the SignalR sample application.</summary>
public static class Program
{
    /// <summary>Runs the sample application.</summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>A task that represents the application lifetime.</returns>
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        _ = builder.Services.AddRazorPages();
        _ = builder.Services.AddSignalR();
        _ = builder.Services.AddCors(static options =>
        {
            options.AddDefaultPolicy(
                static policyBuilder =>
                {
                    _ = policyBuilder.WithOrigins("https://example.com")
                        .AllowAnyHeader()
                        .WithMethods("GET", "POST")
                        .AllowCredentials();
                });
        });

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            _ = app.UseExceptionHandler("/Error");
            _ = app.UseHsts();
        }

        _ = app.UseHttpsRedirection();
        _ = app.UseStaticFiles();
        _ = app.UseRouting();
        _ = app.UseAuthorization();

        // UseCors must be called before MapHub.
        _ = app.UseCors();
        _ = app.MapRazorPages();
        _ = app.MapHub<ChatHub>("/chatHub");

        await app.RunAsync().ConfigureAwait(false);
    }
}
