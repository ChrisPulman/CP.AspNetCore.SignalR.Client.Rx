# CP.AspNetCore.SignalR.Client.Rx
Reactive Extensions for Microsoft.AspNetCore.SignalR.Client

## Installation

```
Install-Package CP.AspNetCore.SignalR.Client.Rx
```

## Usage

```csharp
using CP.AspNetCore.SignalR.Client.Rx;
```

```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/hub")
    .AutoReconnect()
    .Build();

var sub = connection.On<string>("Stream").Subscribe(Console.WriteLine);

var con = connection.StartAsync().Subscribe();
```
