// Copyright (c) Chris Pulman. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#define DEFAULT
#if DEFAULT

using System;
using System.Windows;
using CP.AspNetCore.SignalR.Client.Rx;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveMarbles.ObservableEvents;

namespace SignalRChatClient
{
    /// <summary>
    /// MainWindow.
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MainWindow : Window
    {
        private IDisposable? _subscription;
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
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

                    x.disposables.Add(connectButton.Events().Click.Subscribe(_ => Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            _subscription?.Dispose();
                            _subscription = connection.StartObservable().Subscribe(_ => Dispatcher.Invoke(() =>
                            {
                                messagesList.Items.Add("Connection started");
                                connectButton.IsEnabled = false;
                                sendButton.IsEnabled = true;
                            }));
                            x.disposables.Add(_subscription);
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
        }
    }

#elif RETRY
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using CP.AspNetCore.SignalR.Client.Rx;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveMarbles.ObservableEvents;

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

                    x.disposables.Add(connectButton.Events().Click.Subscribe(async _ =>
                    {
                        connection.On<string, string>("ReceiveMessage").Subscribe(x => Dispatcher.Invoke(() =>
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

                    x.disposables.Add(sendButton.Events().Click.Subscribe(async _ =>
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
    }
#endif
}
