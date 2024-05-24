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

## Other options
    
```csharp
    HubBuilder.Create(builder => builder.WithUrl("https://localhost:53933/ChatHub"))
    .Subscribe(x =>
    {
        var connection = x.hubConnection;
        x.disposables.Add(connection.On<string, string>("ReceiveMessage").Subscribe(responce => Dispatcher.Invoke(() =>
        {
            var newMessage = $"{responce.t1}: {responce.t2}";
            messagesList.Items.Add(newMessage);
        })));

        x.disposables.Add(connection.HasClosed().Subscribe(error => Dispatcher.Invoke(() =>
        {
            connectButton.IsEnabled = true;
            sendButton.IsEnabled = false;
            messagesList.Items.Add(error);
        })));

        x.disposables.Add(connectButton.Events().Click.Start(connection).Subscribe(_ => Dispatcher.Invoke(() =>
        {
            try
            {
                messagesList.Items.Add("Connection started");
                connectButton.IsEnabled = false;
                sendButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
        })));

        x.disposables.Add(sendButton.Events().Click.Subscribe(_ => Dispatcher.Invoke(async () =>
        {
            try
            {
                await connection.InvokeAsync("SendMessage", userTextBox.Text, messageTextBox.Text);
            }
            catch (Exception ex)
            {
                messagesList.Items.Add(ex.Message);
            }
        })));
    });
```

## Available operators

- `On<T>(string methodName)`
- `On<T1, T2>(string methodName)`
- `On<T1, T2, T3>(string methodName)`
- `On<T1, T2, T3, T4>(string methodName)`
- `On<T1, T2, T3, T4, T5>(string methodName)`
- `On<T1, T2, T3, T4, T5, T6>(string methodName)`
- `On<T1, T2, T3, T4, T5, T6, T7>(string methodName)`
- `On<T1, T2, T3, T4, T5, T6, T7, T8>(string methodName)`

- `HasClosed()`
- `HasReconnected()`
- `IsReconnecting()`
- `StartObservable()`
- `StartObservable(CancellationToken cancellationToken)`
- `Start()`
- `Start(CancellationToken cancellationToken)`
- `Start(HubConnection connection)`
- `Start(HubConnection connection, CancellationToken cancellationToken)`
- `StopObservable()`
- `StreamObservable()`

- `InvokeAsync(string methodName, params object[] args)`

- `HubBuilder.Create(builder => // Configure Builder)`
