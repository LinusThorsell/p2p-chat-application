using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TDDD49Template.Models
{
    public interface IMessageService
    {
        bool Show(string title, string message, MessageBoxButton button);
    }

    public sealed class MessageService : IMessageService
    {
        public bool Show(string title, string message, MessageBoxButton button)
        {
            return MessageBox.Show(message, title,
                        button) == MessageBoxResult.Yes;
        }
    }

    public class UserInteractionMessage : MessageBase
    {
        public string Message { get; set; }
        public string Title { get; set; }
        public MessageBoxButton Button { get; set; }
    }
    public class MessagePacketReceived : MessageBase
    {
        public bool hasAccepted { get; set; }
        public string Message { get; set; }
    }
}
