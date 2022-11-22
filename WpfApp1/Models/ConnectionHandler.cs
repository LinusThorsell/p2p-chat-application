using System;
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

        public ObservableCollection<MessagePacket>? MessagePackets { get; protected set; } = new ObservableCollection<MessagePacket>();
        public ObservableCollection<ConversationStore.Conversation> PastConversations;
        /*public List<MessagePacket>? MessagePackets { get; protected set; } = new List<MessagePacket>();*/
        public void AddMessage(MessagePacket messagePacket)
        {
            MessagePackets.Add(messagePacket);
        }
        
        public void ExitChat()
        {
            System.Diagnostics.Debug.WriteLine("Attempting to exit chat");
            // send connectionclose event.
            SendJSON(new MessagePacket()
            {
                RequestType = "closeconnection",
                Name = displayname,
                Date = DateTime.Now,
                Message = ""
            });

            // save conversation to log.
            if (MessagePackets.Count > 0)
            {
                conversationStore.SaveConversation(MessagePackets, displayname, partner);
                MessagePackets.Clear();
            }

            // enter conversation viewer mode.
            System.Diagnostics.Debug.WriteLine("TODO: Entering viewer mode");
            PastConversations = conversationStore.getConverstations();
            
        }
        
/*        public void Passcollection(ObservableCollection<MessagePacket> msgpacket)
        {
            messages = msgpacket;
        }*/
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
                        hasAccepted = true;
                        partner = messagepacket.Name;

                        // Init messages collection
                        /*MessagePackets = new();*/

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

                        // TODO: Enter 'chatting mode'
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
                MessageBox.Show(messagepacket.Name + " has accepted your Chat Request! Enjoy chatting!");
                /*MessagePackets = new();*/
                return 1;
            }
            else if (messagepacket.RequestType == "rejectconnection")
            {
                MessageBox.Show("Your Chat request was denied. Try connecting to someone else!");
                // TODO: reset socket
                return -1; // maybe works? needs testing
            }
            else if (messagepacket.RequestType == "message")
            {
                // If we have a message, we create the message bubble.
                //MessageBox.Show(messagepacket.Message); // TODO: add to display instead
                                                        //MessagePackets.Add(messagepacket);
                Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    MessagePackets.Add(messagepacket);
                }));
            }

            // if we have a connectionclose event we close the communications.
            // TODO: Implement

            return 1;
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
                            keepConnection = false;
                        };
                    }
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Could not connect to TcpListener");
                MessageBox.Show("Could not connect, or lost connection while connected.");
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

                    using TcpClient handler = await listener.AcceptTcpClientAsync();
                    stream = handler.GetStream();


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
            }
            finally
            {
                listener.Stop();
            }

        }
    }
}
