﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TDDD49Template.Models;
using WpfApp1.Models;
using WpfApp1.ViewModels.Commands;

namespace WpfApp1.ViewModels
{
    public class MainViewModel
    {
        private ConnectionHandler _connection;

        private String _messageToSend;
        private String _portToConnect;
        private String _ipToConnect;
        private String _portToListen;
        private String _displayname;

        private ICommand _pushSendMessage;
        private ICommand _pushConnect;
        private ICommand _pushListen;
        private ICommand _pushDisplayname;
        private ICommand _pushExitChat;

        public ConnectionHandler Connection
        {
            get { return _connection; }
            set
            {
                _connection = value;
            }
        }

        public String MessageToSend
        {
            get { return _messageToSend; }
            set { _messageToSend = value; }
        }
        public String PortToConnect
        {
            get { return _portToConnect; }
            set { _portToConnect = value; }
        }
        public String IpToConnect
        {
            get { return _ipToConnect; }
            set { _ipToConnect = value; }
        }
        public String PortToListen
        {
            get { return _portToListen; }
            set { _portToListen = value; }
        }
        public String Displayname
        {
            get { return _displayname; }
            set { _displayname = value; }
        }

        public ICommand PushSendMessage
        {
            get { return _pushSendMessage; }
            set { _pushSendMessage = value; }
        }
        public ICommand PushConnect
        {
            get { return _pushConnect; }
            set { _pushConnect = value; }
        }
        public ICommand PushListen
        {
            get { return _pushListen; }
            set { _pushListen = value; }
        }
        public ICommand PushDisplayname
        {
            get { return _pushDisplayname; }
            set { _pushDisplayname = value; }
        }
        public ICommand PushExitChat
        {
            get { return _pushExitChat; }
            set { _pushExitChat = value; }
        }

        public MainViewModel(ConnectionHandler connectionHandler)
        {
            this.Connection = connectionHandler;

            this.PushSendMessage = new SendMessageCommand(this);
            this.PushConnect = new ConnectCommand(this);
            this.PushListen = new ListenCommand(this);
            this.PushDisplayname = new DisplaynameCommand(this);
            this.PushExitChat = new ExitChatCommand(this);
        }
        public void sendMessage()
        {
            Connection.SendMessage(MessageToSend);
        }
        public void connect()
        {
            Connection.ConnectToListener(_ipToConnect, _portToConnect);
        }
        public void listen()
        {
            Connection.StartListeningOnPort(_portToListen);
        }
        public void setDisplayname()
        {
            Connection.SetDisplayname(_displayname);
        }
        public void exitChat()
        {
            Connection.ExitChat();
        }
    }
}