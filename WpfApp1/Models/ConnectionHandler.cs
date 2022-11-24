﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Interop;
using System.Net.Sockets;
using System.Text.Json;
using Microsoft.VisualBasic;
using System.Xml.Linq;
using System.Collections.ObjectModel;

using TDDD49Template.Models;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Reflection.Metadata.Ecma335;
using System.Windows.Input;
using static TDDD49Template.Models.ConversationStore;

namespace WpfApp1.Models
{
    public class ConnectionHandler
    {
        TcpClient client;
        TcpListener listener;
        string displayname;
        string partner;
        NetworkStream stream;
        bool hasAccepted = false;
        ConversationStore conversationStore = new() {};
        private string CurrentConversationID = "";

        public ObservableCollection<MessagePacket>? MessagePackets { get; protected set; } = new ObservableCollection<MessagePacket>();
        public ObservableCollection<ConversationStore.Conversation>? PastConversations { get; protected set; } = new ObservableCollection<ConversationStore.Conversation>();
        /*public List<MessagePacket>? MessagePackets { get; protected set; } = new List<MessagePacket>();*/

        public void showPastChat(ConversationStore.Conversation conversation)
        {
            // TODO force user to select chatwindow tabitem.
            
            
            
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                ObservableCollection<MessagePacket> temp_convo = conversationStore.getConversationsById(conversation.Id, displayname);

                MessagePackets.Clear();
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

        public void AddMessage(MessagePacket messagePacket)
        {
            MessagePackets.Add(messagePacket);
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

            // enter conversation viewer mode.
            UpdatePastChats();

            // close connection.
            hasAccepted = false;

            // close stream.
            
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
            // Here is the code which sends the data over the network.
            // No user interaction shall exist in the model.

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
        private void SendJSON(MessagePacket messagepacket)
        {
            var serialized_messagepacket = (JsonSerializer.Serialize<MessagePacket>(messagepacket));
            var encoded_messagepacket = Encoding.UTF8.GetBytes(serialized_messagepacket);
            stream.WriteAsync(encoded_messagepacket);
        }
        public void SetDisplayname(string name)
        {
            displayname = name;
            UpdatePastChats();
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

        private int ReceiveMessage(string jsonobj)
        {
            System.Diagnostics.Debug.WriteLine($"Message received: \"{jsonobj}\"");

            // TODO: Decode the JSON string into object.
            MessagePacket? messagepacket = JsonSerializer.Deserialize<MessagePacket>(jsonobj);

            if (messagepacket == null) { return -1; }   // Guard for null values (shouldnt happen using the program)
                                                        // but would stop some attack vectors

            // If we have a establishconnection request, ask the receiver if they want to connect, otherwise disconnect socket.
            if (messagepacket.RequestType == "establishconnection")
            {
                if (!hasAccepted)
                {
                    if (MessageBox.Show("Do you want to accept the chat request from user " + messagepacket.Name + "?",
                        "Message Request Incoming",
                        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        // User clicked yes

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
                        return 1;

                    }
                    else
                    {
                        // User clicked no
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
                        return -1;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"establishconnection received while already connected: \"{jsonobj}\"");
                }
            }
            else if (messagepacket.RequestType == "acceptconnection")
            {
                // Remove all current chat messages in the window.
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MessagePackets.Clear();
                }));

                partner = messagepacket.Name;
                CurrentConversationID = displayname + partner + DateTime.Now.ToString("HH:mm:ss");
                MessageBox.Show(messagepacket.Name + " has accepted your Chat Request! Enjoy chatting!");
                return 1;
            }
            else if (messagepacket.RequestType == "rejectconnection")
            {
                MessageBox.Show("Your Chat request was denied. Try connecting to someone else!");
                return -1; // maybe works? needs testing
            }
            else if (messagepacket.RequestType == "message")
            {
                // If we have a message, we create the message bubble.
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MessagePackets.Add(messagepacket);
                }));
                conversationStore.UpdateConversation(CurrentConversationID, MessagePackets, displayname, partner);
                UpdatePastChats();
            }
            else if (messagepacket.RequestType == "closeconnection")
            {
                // If we have a closeconnection request, we close the connection.
                return -2; // maybe works? needs testing
            }

            return 1;
        }

        private void handleConnectionClosed()
        {
            if (CurrentConversationID.Length > 1)
            {
                // We were in a conversation when the Listener failed.
                MessageBox.Show(partner + " disconnected from the chat.");
            }
            else
            {
                // We were in the process of connecting to a partner when the Listener failed.
                MessageBox.Show("Connection to partner failed. Try connecting to someone else!");

            }

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
                //var message = JsonSerializer.Serialize(obj); // Make json object into string
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
                        if (ReceiveMessage(received_messagepacket) == -1) // if we have been denied close the socket
                        {
                            System.Diagnostics.Debug.WriteLine("Chat denied, closing socket");
                            MessageBox.Show("Receiver denied the chat request.");
                            keepConnection = false;
                        };
                        if (ReceiveMessage(received_messagepacket) == -2)
                        {
                            keepConnection = false;
                            MessageBox.Show(partner + " disconnected from the chat.");
                            //handleConnectionClosed();
                            CurrentConversationID = "";
                        }
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Could not connect to TcpListener");
                //handleConnectionClosed();
            }

        }
        private async void StartTcpListener(string port)
        {

            var ipEndPoint = new IPEndPoint(IPAddress.Any, int.Parse(port));
            listener = new(ipEndPoint);

            try
            {
                while (true) // After you have selected listen, keep listening forever 
                             // TODO: Make it exitable.
                {
                    listener.Start();
                    System.Diagnostics.Debug.WriteLine("after listener.start()");

                    System.Diagnostics.Debug.WriteLine("before accept");
                    using TcpClient handler = await listener.AcceptTcpClientAsync();
                    System.Diagnostics.Debug.WriteLine("after accept");
                    System.Diagnostics.Debug.WriteLine("before getstream");
                    stream = handler.GetStream();
                    System.Diagnostics.Debug.WriteLine("after getstream");



                    var keepConnection = true;
                    while (keepConnection)
                    {
                        var buffer = new byte[1_024];
                        int received = await stream.ReadAsync(buffer);
                        var message = Encoding.UTF8.GetString(buffer, 0, received);

                        if (received > 0)
                        {
                            if (ReceiveMessage(message) == -1)
                            {
                                // User denied connection request => stop listening for their messages.
                                listener.Stop();
                                // Break loop to relisten for next connection request.
                                keepConnection = false;
                            };
                        }
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Could not start listening/connect to TCPClient");

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
