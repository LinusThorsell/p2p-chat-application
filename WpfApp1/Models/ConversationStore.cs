using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WpfApp1.ViewModels.Commands;

namespace TDDD49Template.Models
{
    public class ConversationStore
    {
        public class Conversation
        {
            public string PartnerName { get; set; }
            public ObservableCollection<MessagePacket> Messages { get; set; }
            public DateTime Date { get; set; }

            public Conversation(DateTime Date, string PartnerName, ObservableCollection<MessagePacket> Messages)
            {
                this.Date = Date;
                this.PartnerName = PartnerName;
                this.Messages = Messages;
            }
        }

        private List<Conversation> Conversations = new List<Conversation>();

        private void UpdateJSONStorage(string MyName, string Partner)
        {
            System.Diagnostics.Debug.WriteLine("Conversations object to write to file:");
            System.Diagnostics.Debug.WriteLine(Conversations);
            string json_output = JsonSerializer.Serialize(Conversations);

            System.Diagnostics.Debug.WriteLine("Writing to file:");
            System.Diagnostics.Debug.WriteLine(json_output);

            // TODO: check if file already exists and append to it instead of rewriting it.
            File.WriteAllText("ConversationsAs_" + MyName + ".json", json_output);
        }

        public void SaveConversation(ObservableCollection<MessagePacket> Messages, string MyName, string Partner)
        {
            DateTime Date = Messages.Last().Date;

            Conversations.Add(new Conversation ( Date, Partner, Messages ));

            UpdateJSONStorage(MyName, Partner);
        }

        public ObservableCollection<Conversation> getConverstations()
        {
            return new ObservableCollection<Conversation>(Conversations);
        }
    }
}