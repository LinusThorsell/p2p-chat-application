using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
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
        private String _queryToSearch;

        private ICommand _pushSendMessage;
        private ICommand _pushConnect;
        private ICommand _pushListen;
        private ICommand _pushDisplayname;
        private ICommand _pushExitChat;
        private ICommand _pushSearchAndUpdatePastConversations;
        private ICommand _pushBuzzz;

           // Mirror data in model
        public ObservableCollection<MessagePacket>? MessagePackets => Connection.MessagePackets;

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
        public String QueryToSearch
        {
            get { return _queryToSearch; }
            set { _queryToSearch = value; }
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
        public ICommand PushSearchAndUpdatePastConversations
        {
            get { return _pushSearchAndUpdatePastConversations; }
            set { _pushSearchAndUpdatePastConversations = value; }
        }

        public ICommand PushShowPastChat => new RelayCommand<ConversationStore.Conversation>(obj => showPastChat(obj));

        public ICommand PushBuzzz
        {
            get { return _pushBuzzz; }
            set { _pushBuzzz = value; }
        }

        public void showPastChat(ConversationStore.Conversation conversation)
        {
            Connection.showPastChat(conversation);
        }

        public MainViewModel(ConnectionHandler connectionHandler)
        {
            this.Connection = connectionHandler;

            this.PushSendMessage = new SendMessageCommand(this);
            this.PushConnect = new ConnectCommand(this);
            this.PushListen = new ListenCommand(this);
            this.PushDisplayname = new DisplaynameCommand(this);
            this.PushExitChat = new ExitChatCommand(this);
            this.PushSearchAndUpdatePastConversations = new SearchAndUpdatePastConversationsCommand(this);
            this.PushBuzzz = new BuzzzCommand(this);
        }

        public void searchAndUpdatePastConversations()
        {
            Connection.SearchAndUpdatePastConversations(_queryToSearch);
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
        public void buzzz()
        {
            Connection.sendBuzzz();
        }
    }
}