using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Net.Sockets;
using System.Text.Json;
using System.Collections.ObjectModel;
using TDDD49Template.Models;
using WpfApp1.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Interop;

namespace WpfApp1.Models
{
    public class ConnectionHandler //: INotifyPropertyChanged
    {
        TcpClient client;
        TcpListener listener;
        string displayname;
        string partner;
        NetworkStream stream;
        bool hasAccepted = false;
        ConversationStore conversationStore = new() {};
        private string CurrentConversationID = "";

        bool stopListening = false;
        bool keepConnection = true;

        public int status = 0;

        public ObservableCollection<MessagePacket>? MessagePackets { get; protected set; } = new ObservableCollection<MessagePacket>();
        public ObservableCollection<ConversationStore.Conversation>? PastConversations { get; protected set; } = new ObservableCollection<ConversationStore.Conversation>();

        // ============== Frontend / ViewModel Functions ============
        public void AcceptIncomingChat(MessagePacket messagepacket)
        {
            // Clear any past messages
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MessagePackets.Clear();
            }));

            hasAccepted = true;
            partner = messagepacket.Name;

            CurrentConversationID = displayname + partner + DateTime.Now.ToString("HH:mm:ss");

            // Create response packet
            MessagePacket? response = new()
            {
                RequestType = "acceptconnection",
                Name = displayname,
                Date = DateTime.Now,
                Message = ""
            };

            // Send response packet
            SendJSON(response);
            status = 1;
        }
        public void DenyIncomingChat()
        {
            // Send denied message to requester

            // Create response packet
            MessagePacket? response = new()
            {
                RequestType = "rejectconnection",
                Name = "",
                Date = DateTime.Now,
                Message = ""
            };

            // Send response packet
            SendJSON(response);

            // Trigger relisten on port number.
            status = -1;
        }
        public void Got_AcceptConnection(MessagePacket messagepacket)
        {
            // Remove all current chat messages in the window.
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MessagePackets.Clear();
            }));

            partner = messagepacket.Name;
            CurrentConversationID = displayname + partner + DateTime.Now.ToString("HH:mm:ss");
            status = 1;
        }
        public void Got_RejectConnection()
        {
            status = -1;
        }
        public void Got_Message(MessagePacket messagepacket)
        {
            // If we have a message, we create the message bubble.
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MessagePackets.Add(messagepacket);
            }));
            conversationStore.UpdateConversation(CurrentConversationID, MessagePackets, displayname, partner);
            UpdatePastChats();
        }
        public void Got_CloseConnection()
        {
            status = -2;
        }
        public void Got_Buzzz()
        {
            receiveBuzzz();
        }

        public void SetDisplayname(string name)
        {
            displayname = name;
            UpdatePastChats();
        }

        public void showPastChat(ConversationStore.Conversation conversation)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ObservableCollection<MessagePacket> temp_convo = conversationStore.getConversationsById(conversation.Id, displayname);

                // Clear currently displayed messages.
                MessagePackets.Clear();

                // Add messages from the selected conversation.
                foreach (MessagePacket conversation in temp_convo)
                {
                    MessagePackets.Add(conversation);
                }
            }));
        }

        public void SearchAndUpdatePastConversations(string query)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                List<ConversationStore.Conversation> temp_convo = conversationStore.SearchAndUpdatePastConversations(query);

                PastConversations.Clear();
                foreach (ConversationStore.Conversation conversation in temp_convo)
                {
                    PastConversations.Add(conversation);
                }
            }));
        }

        public void sendBuzzz()
        {
            SendJSON(new MessagePacket()
            {
                RequestType = "buzzz",
                Name = displayname,
                Date = DateTime.Now,
                Message = ""
            });
        }
        public void ExitChat()
        {
            // send connectionclose event.
            SendJSON(new MessagePacket()
            {
                RequestType = "closeconnection",
                Name = displayname,
                Date = DateTime.Now,
                Message = ""
            });

            // Clear Messages
            MessagePackets.Clear();

            // close connection.
            hasAccepted = false;

            // close listening outer loop.
            stopListening = true;
            // close listening inner loop
            keepConnection = false;
            // close actual listener
            if (listener != null)
            {
                listener.Stop();
            }

            // close stream.
            handleConnectionClosed();
            
        }

        public void ConnectToListener(string ip, string port)
        {
            Thread connectionThread = new Thread(() => ConnectTcpClientToListener(ip, port));
            connectionThread.Start();
        }
        public void StartListeningOnPort(string port)
        {
            Thread listeningThread = new Thread(() => StartTcpListener(port));
            listeningThread.Start();
        }
        public void SendMessage(String msg)
        {

            MessagePacket? messagepacket = new()
            {
                RequestType = "message",
                Name = displayname,
                Date = DateTime.Now,
                Message = msg
            };

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MessagePackets.Add(messagepacket);
            }));

            conversationStore.UpdateConversation(CurrentConversationID, MessagePackets, displayname, partner);
            UpdatePastChats();

            var serialized_messagepacket = (JsonSerializer.Serialize<MessagePacket>(messagepacket));
            var encoded_messagepacket = Encoding.UTF8.GetBytes(serialized_messagepacket);
            stream.WriteAsync(encoded_messagepacket);

        }

        // =================== Backend =========================
        private void SendJSON(MessagePacket messagepacket)
        {
            var serialized_messagepacket = (JsonSerializer.Serialize<MessagePacket>(messagepacket));
            var encoded_messagepacket = Encoding.UTF8.GetBytes(serialized_messagepacket);
            if (stream != null)
            {
                stream.WriteAsync(encoded_messagepacket);
            }
        }
        private void UpdatePastChats()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                List<ConversationStore.Conversation> temp_convo = conversationStore.getConversations(displayname);

                PastConversations.Clear();
                foreach (ConversationStore.Conversation conversation in temp_convo)
                {
                    PastConversations.Add(conversation);
                }
            }));
        }
        public void AddMessage(MessagePacket messagePacket)
        {
            MessagePackets.Add(messagePacket);
        }

        private void receiveBuzzz()
        {
            //the wav filename
            string file = "pling.wav";

            //get the current assembly
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();

            //load the embedded resource as a stream
            var stream = assembly.GetManifestResourceStream(string.Format("{0}.Res.{1}", assembly.GetName().Name, file));

            //load the stream into the player
            var player = new System.Media.SoundPlayer(stream);

            //play the sound
            player.Play();
        }
        private void handleConnectionClosed()
        {
            // Reset variables
            partner = "";

            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                MessagePackets.Clear();
            }));

            hasAccepted = false;
            CurrentConversationID = "";
        }

        // =================== Threaded tasks/functions =======================

        private async void ConnectTcpClientToListener(string ip, string port)
        {
            // get string ip, string port
            System.Diagnostics.Debug.WriteLine("Thread: connectTCPClientToListener started with ip=" + ip + " port=" + port);

            MessagePacket packet = new()
            {
                RequestType = "establishconnection",
                Name = displayname,
                Date = DateTime.Now,
                Message = ""
            };

            try
            {
                var ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));

                client = new TcpClient();

                await client.ConnectAsync(ipEndPoint);
                stream = client.GetStream();
                
                hasAccepted = true; // We assume that the person requesting the connection doesnt need to approve it again.
                var serialized_messagepacket = JsonSerializer.Serialize<MessagePacket>(packet);
                var bytestringed_messagepacket = Encoding.UTF8.GetBytes(serialized_messagepacket); // Encode string to sendable bytes
                await stream.WriteAsync(bytestringed_messagepacket); // Send bytes over stream.

                var keepConnection = true;
                while (keepConnection)
                {
                    var buffer = new byte[1_024]; // Buffer to read into
                    int received = await stream.ReadAsync(buffer); // Check if we have received something
                    var received_messagepacket = Encoding.UTF8.GetString(buffer, 0, received); // Decode message into UTF8 string
                     
                    if (received > 0) // If the string is larger than 0 = received something usable.
                    {
                        Messenger.Default.Send<MessagePacketReceived>(new MessagePacketReceived() {hasAccepted=hasAccepted, Message=received_messagepacket });

                        if (status == -1) // if we have been denied close the socket
                        {
                            System.Diagnostics.Debug.WriteLine("Chat denied, closing socket");
                            keepConnection = false;
                        };
                        if (status == -2)
                        {
                            keepConnection = false;
                            Messenger.Default.Send<UserInteractionMessage>(new UserInteractionMessage() { Title = "Notification", Message = (partner + " disconnected from the chat."), Button = MessageBoxButton.OK });
                            handleConnectionClosed();
                            CurrentConversationID = "";
                        }
                    }
                }
            }
            catch
            {
                Messenger.Default.Send<UserInteractionMessage>(new UserInteractionMessage() { Title = "Notification", Message = (partner + " disconnected from the chat."), Button=MessageBoxButton.OK });
                System.Diagnostics.Debug.WriteLine("Could not connect to TcpListener");
                handleConnectionClosed();
            }

        }
        private async void StartTcpListener(string port)
        {

            var ipEndPoint = new IPEndPoint(IPAddress.Any, int.Parse(port));
            listener = new(ipEndPoint);
            stopListening = false;

            try
            {
                while (!stopListening) // After you have selected listen, keep listening forever 
                {
                    listener.Start();
                    System.Diagnostics.Debug.WriteLine("after listener.start()");

                    System.Diagnostics.Debug.WriteLine("before accept");
                    using TcpClient handler = await listener.AcceptTcpClientAsync();
                    System.Diagnostics.Debug.WriteLine("after accept");
                    System.Diagnostics.Debug.WriteLine("before getstream");
                    stream = handler.GetStream();
                    System.Diagnostics.Debug.WriteLine("after getstream");



                    keepConnection = true;
                    while (keepConnection)
                    {
                        var buffer = new byte[1_024];
                        int received = await stream.ReadAsync(buffer);
                        var message = Encoding.UTF8.GetString(buffer, 0, received);

                        if (received > 0)
                        {
                            Messenger.Default.Send<MessagePacketReceived>(new MessagePacketReceived() { hasAccepted = hasAccepted, Message = message });

                            if (status == -1)
                            {
                                // User denied connection request => stop listening for their messages.
                                listener.Stop();
                                // Break loop to relisten for next connection request.
                                keepConnection = false;
                            };
                            if (status == -2)
                            {
                                listener.Stop();
                                // Break loop to relisten for next connection request.
                                keepConnection = false;
                                Messenger.Default.Send<UserInteractionMessage>(new UserInteractionMessage() { Title = "Notification", Message = (partner + " disconnected from the chat."), Button = MessageBoxButton.OK });
                                handleConnectionClosed();
                                CurrentConversationID = "";
                            };
                        }
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Could not start listening/connect to TCPClient");
                if (partner != "")
                {
                    Messenger.Default.Send<UserInteractionMessage>(new UserInteractionMessage() { Title = "Notification", Message = (partner + " disconnected from the chat."), Button = MessageBoxButton.OK });
                }
                else
                {
                    Messenger.Default.Send<UserInteractionMessage>(new UserInteractionMessage() { Title = "Notification", Message = "Unknown conection error, closing connection.", Button = MessageBoxButton.OK });
                }

                listener.Stop();
                handleConnectionClosed();
            }
            finally
            {
                listener.Stop();
            }

        }
    }
}
