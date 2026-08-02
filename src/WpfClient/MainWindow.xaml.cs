// Copyright (c) 2019-2026 Chris Pulman and contributors. All rights reserved.
// Chris Pulman and contributors licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

#define DEFAULT
#if DEFAULT

namespace SignalRChatClient;

/// <summary>Main application window.</summary>
/// <seealso cref="Window" />
/// <seealso cref="System.Windows.Markup.IComponentConnector" />
public partial class MainWindow : Window
{
    /// <summary>Initializes a new instance of the <see cref="MainWindow"/> class.</summary>
    public MainWindow()
    {
        InitializeComponent();
        _ = HubBuilder.Create(static builder => builder.WithUrl("https://localhost:53933/ChatHub"))
            .Subscribe(x =>
            {
                var connection = x.hubConnection;
                x.disposables.Add(connection.On(new HubMethod<(string, string)>("ReceiveMessage")).Subscribe(response => Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{response.t1}: {response.t2}";
                    _ = messagesList.Items.Add(newMessage);
                })));

                x.disposables.Add(connection.HasClosed().Subscribe(error => Dispatcher.Invoke(() =>
                {
                    connectButton.IsEnabled = true;
                    sendButton.IsEnabled = false;
                    _ = messagesList.Items.Add(error);
                })));

                x.disposables.Add(ObserveClick(connectButton).Start(connection).Subscribe(startedConnection => Dispatcher.Invoke(() =>
                {
                    Debug.Assert(startedConnection.State == HubConnectionState.Connected, "Connected");
                    try
                    {
                        _ = messagesList.Items.Add("Connection started");
                        connectButton.IsEnabled = false;
                        sendButton.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        _ = messagesList.Items.Add(ex.Message);
                    }
                })));

                x.disposables.Add(ObserveClick(sendButton)
                    .SelectMany(_ => connection.InvokeObservable("SendMessage", default, userTextBox.Text, messageTextBox.Text))
                    .Subscribe(
                        static _ => Debug.WriteLine("Message sent."),
                        error => Dispatcher.Invoke(() => _ = messagesList.Items.Add(error.Message))));
            });
    }

    /// <summary>Creates an observable sequence for a button's click event.</summary>
    /// <param name="button">The observed button.</param>
    /// <returns>An observable sequence of routed event arguments.</returns>
    private static IObservable<RoutedEventArgs> ObserveClick(Button button) =>
        Observable.CreateSafe<RoutedEventArgs>(observer =>
        {
            RoutedEventHandler handler = (_, eventArgs) => observer.OnNext(eventArgs);
            button.Click += handler;
            return Disposable.Create((button, handler), static state => state.button.Click -= state.handler);
        });
}

#elif RETRY
namespace SignalRChatClient
{
/// <summary>
/// MainWindow.
/// </summary>
/// <seealso cref="System.Windows.Window" />
/// <seealso cref="System.Windows.Markup.IComponentConnector" />
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();
        HubBuilder.Create(builder =>
            builder.WithUrl("https://localhost:53933/ChatHub")
                   .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(10) }))
            .Subscribe(x =>
            {
                var connection = x.hubConnection;
                x.disposables.Add(connection.HasClosed().Subscribe(error =>
                    Debug.Assert(connection.State == HubConnectionState.Disconnected, "Disconnected")));

                x.disposables.Add(connection.IsReconnecting().Subscribe(_ =>
                    Debug.Assert(connection.State == HubConnectionState.Reconnecting, "Reconnecting")));

                x.disposables.Add(connection.HasReconnected().Subscribe(_ =>
                    Debug.Assert(connection.State == HubConnectionState.Connected, "Connected")));

                x.disposables.Add(ObserveClick(connectButton).Subscribe(async _ =>
                {
                    connection.On(new HubMethod<(string, string)>("ReceiveMessage")).Subscribe(x => Dispatcher.Invoke(() =>
                    {
                        var newMessage = $"{x.t1}: {x.t2}";
                        messagesList.Items.Add(newMessage);
                    }));

                    try
                    {
                        ConnectWithRetryAsync(connection, default);
                        messagesList.Items.Add("Connection started");
                        connectButton.IsEnabled = false;
                        sendButton.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        messagesList.Items.Add(ex.Message);
                    }
                }));

                x.disposables.Add(ObserveClick(sendButton).Subscribe(async _ =>
                {
                    try
                    {
                        await connection.InvokeAsync("SendMessage", userTextBox.Text, messageTextBox.Text);
                    }
                    catch (Exception ex)
                    {
                        messagesList.Items.Add(ex.Message);
                    }
                }));

                static void ConnectWithRetryAsync(HubConnection connection, CancellationToken token)
                {
                    connection.StartObservable(token).Subscribe(
                    _ =>
                    {
                        Debug.Assert(connection.State == HubConnectionState.Connected, "Connected");
                    },
                    ex =>
                    {
                        if (ex is OperationCanceledException)
                        {
                            Debug.Assert(token.IsCancellationRequested, "Canceled");
                        }
                        else
                        {
                            Debug.Assert(connection.State == HubConnectionState.Disconnected, "Disconnected");
                        }
                    },
                    () => { });
                }
            });
    }

    private static IObservable<RoutedEventArgs> ObserveClick(Button button) =>
        Observable.CreateSafe<RoutedEventArgs>(observer =>
        {
            RoutedEventHandler handler = (_, eventArgs) => observer.OnNext(eventArgs);
            button.Click += handler;
            return Disposable.Create((button, handler), static state => state.button.Click -= state.handler);
        });
}
#endif
