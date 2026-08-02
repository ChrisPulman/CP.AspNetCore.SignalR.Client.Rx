# CP.AspNetCore.SignalR.Client.Rx
ReactiveUI.Primitives extensions for Microsoft.AspNetCore.SignalR.Client.

## Installation

```powershell
Install-Package CP.AspNetCore.SignalR.Client.Rx
```

Use the lean package above when the application does not otherwise depend on System.Reactive. Applications that require System.Reactive interoperability can install the shared-source variant:

```powershell
Install-Package CP.AspNetCore.SignalR.Client.Rx.Reactive
```

## Overview

CP.AspNetCore.SignalR.Client.Rx provides high-performance, memory-efficient observable APIs for SignalR client applications. The lean package is based on `ReactiveUI.Primitives`; the `.Reactive` package uses `ReactiveUI.Primitives.Reactive` and the `CP.AspNetCore.SignalR.Client.Rx.Reactive` namespace.

- **Targets:** .NET Framework 4.6.2, .NET 8, .NET 9, .NET 10, and .NET 11
- **Supports:** WPF, WinForms, Console, ASP.NET Core, Razor Pages, and more

## Quick Start

```csharp
using CP.AspNetCore.SignalR.Client.Rx;
using Microsoft.AspNetCore.SignalR.Client;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/hub")
    .WithAutomaticReconnect()
    .Build();

// Subscribe to a hub method
var sub = connection.On(new HubMethod<string>("Stream")).Subscribe(Console.WriteLine);

// Start the connection reactively
connection.StartObservable().Subscribe();
```

## Advanced Usage

### Using HubBuilder for Lifecycle Management

```csharp
using CP.AspNetCore.SignalR.Client.Rx;

HubBuilder.Create(builder => builder.WithUrl("https://localhost:53933/ChatHub"))
    .Subscribe(x =>
    {
        var connection = x.hubConnection;
        var disposables = x.disposables;

        // Listen for messages
        disposables.Add(connection.On(new HubMethod<(string, string)>("ReceiveMessage"))
            .Subscribe(msg => Console.WriteLine($"{msg.t1}: {msg.t2}")));

        // Handle connection closed
        disposables.Add(connection.HasClosed().Subscribe(error =>
            Console.WriteLine($"Closed: {error?.Message}")));

        // Start the connection from any observable trigger.
        // disposables.Add(connectRequested.Start(connection).Subscribe());
    });
```

### Streaming from the Server

```csharp
connection.StreamObservable(new HubMethod<int>("Counter"), CancellationToken.None, 10)
    .Subscribe(
        value => Console.WriteLine($"Received: {value}"),
        ex => Console.WriteLine($"Error: {ex.Message}"),
        () => Console.WriteLine("Stream completed"));
```

### Invoking Methods and Sending Data

```csharp
// Invoke a method and get a result
connection.InvokeObservable(new HubMethod<int>("AddNumbers"), CancellationToken.None, 1, 2)
    .Subscribe(result => Console.WriteLine($"Sum: {result}"));

// Send a method (fire-and-forget)
connection.SendObservable("Notify", CancellationToken.None, "Hello!")
    .Subscribe(_ => Console.WriteLine("Notification sent"));
```

### Observing Connection State

```csharp
connection.StateChanges().Subscribe(state =>
    Console.WriteLine($"Connection state: {state}"));

// Wait for a specific state
connection.WaitForState(HubConnectionState.Connected)
    .Subscribe(_ => Console.WriteLine("Connected!"));
```

### Razor Pages Example

```csharp
@page
@using CP.AspNetCore.SignalR.Client.Rx
@inject Microsoft.AspNetCore.SignalR.Client.HubConnection Connection

@functions {
    protected override void OnInitialized()
    {
        Connection.On(new HubMethod<string>("ReceiveMessage"))
            .Subscribe(msg => /* update UI */);
        Connection.StartObservable().Subscribe();
    }
}
```

## API Reference

### Event Subscription
- `On(string methodName)` — No-arg handler
- `On<T>(HubMethod<T> method)`
- `On<T1, T2>(HubMethod<(T1, T2)> method)`
- ... up to 8 arguments

### Connection Lifecycle
- `StartObservable()` / `StartObservable(CancellationToken)`
- `StartObservable(int? retryCount, CancellationToken)`
- `StopObservable()` / `StopObservable(CancellationToken)`
- `StopObservable(int? retryCount, CancellationToken)`
- `EnsureStarted()` / `EnsureStarted(CancellationToken)`
- `StateChanges()`
- `WaitForState(HubConnectionState)`

### Streaming
- `StreamObservable<T>(HubMethod<T> method, CancellationToken, params object?[] args)`

### Invocation
- `InvokeObservable<T>(HubMethod<T> method, CancellationToken, params object?[] args)`
- `InvokeObservable(string methodName, CancellationToken, params object?[] args)`
- `SendObservable(string methodName, CancellationToken, params object?[] args)`

### Connection Events
- `HasClosed()`
- `IsReconnecting()`
- `HasReconnected()`

### HubBuilder
- `HubBuilder.Create(Func<HubConnectionBuilder, IHubConnectionBuilder>)`

## Best Practices
- Always dispose subscriptions to avoid memory leaks.
- Use the disposable scope returned by `HubBuilder.Create` for managing multiple subscriptions.
- Prefer `StartObservable`/`StopObservable` for connection management in Rx scenarios.
- Use `StreamObservable` for server-to-client streaming.

## License
MIT

## See Also
- [Microsoft.AspNetCore.SignalR.Client Documentation](https://learn.microsoft.com/aspnet/core/signalr/dotnet-client)
- [Reactive Extensions (Rx)](https://github.com/dotnet/reactive)

---


**CP.AspNetCore.SignalR.Client.Rx** - Empowering Industrial Automation with Reactive Technology ⚡🏭
