using System;
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
        //public ObservableCollection<MessagePacket> MessagePackets { get; protected set; }

        private String _messageToSend;
        private String _portToConnect;
        private String _ipToConnect;
        private String _portToListen;
        private String _displayname;

        private ICommand _pushSendMessage;
        private ICommand _pushConnect;
        private ICommand _pushListen;
        private ICommand _pushDisplayname;

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

        public MainViewModel(ConnectionHandler connectionHandler)
        {
            this.Connection = connectionHandler;
            
            //if (Connection.MessagePackets == null) { System.Diagnostics.Debug.WriteLine("MessagePackets is null"); }
            //this.MessagePackets = new ObservableCollection<MessagePacket>(Connection.MessagePackets);

            this.PushSendMessage = new SendMessageCommand(this);
            this.PushConnect = new ConnectCommand(this);
            this.PushListen = new ListenCommand(this);
            this.PushDisplayname = new DisplaynameCommand(this);
        }
/*        public void AddMessage()
        {
            this.Connection.AddMessage(new MessagePacket());
            //What should I do here?
        }*/
        public void sendMessage()
        {
/*            System.Diagnostics.Debug.WriteLine("List in View");
            for (int i = 0; i < Connection.MessagePackets.Count; i++)
            {
                System.Diagnostics.Debug.WriteLine(Connection.MessagePackets[i].Message);
            }
            System.Diagnostics.Debug.WriteLine("End of: List in View");*/

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
    }
}