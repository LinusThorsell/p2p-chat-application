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
using static TDDD49Template.Models.ConversationStore;

namespace TDDD49Template.Models
{
    public class ConversationStore
    {
        public class Conversation
        {
            public string Id { get; set; }
            public string PartnerName { get; set; }
            public ObservableCollection<MessagePacket> Messages { get; set; }
            public DateTime Date { get; set; }

            public Conversation(string Id, DateTime Date, string PartnerName, ObservableCollection<MessagePacket> Messages)
            {
                this.Id = Id;
                this.Date = Date;
                this.PartnerName = PartnerName;
                this.Messages = Messages;
            }
        }

        private List<Conversation> Conversations = new();

        private void WriteConversationsToJSONStorage(string MyName)
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

            Conversations.Add(new Conversation ( "ecksdee", Date, Partner, Messages ));

            //WriteJSONStorage(MyName);
        }

        public void UpdateConversation(string ConversationID, ObservableCollection<MessagePacket> Messages, string MyName, string Partner)
        {
            System.Diagnostics.Debug.WriteLine("Updating Conversation: ", ConversationID);

            // Check if logging file exists for current name.
            if (!File.Exists("ConversationsAs_" + MyName + ".json"))
            {
                // File does not exist.
                
                // Make file.
                Conversations.Add(new Conversation(ConversationID, DateTime.Now, Partner, Messages));
                WriteConversationsToJSONStorage(MyName);
            }
            else
            {
                // File exists.

                // Read the file and deserialize JSON to a type.
                string json = File.ReadAllText("ConversationsAs_" + MyName + ".json");
                Conversations = JsonSerializer.Deserialize<List<Conversation>>(json);

                // Check if conversation already exists.
                if (Conversations.Exists(x => x.Id == ConversationID))
                {
                    // Update the conversation.
                    Conversations.Find(x => x.Id == ConversationID).Messages = Messages;
                }
                else
                {
                    // Add the conversation.
                    Conversations.Add(new Conversation(ConversationID, DateTime.Now, Partner, Messages));
                }

                WriteConversationsToJSONStorage(MyName);
            }
        }

        public List<Conversation> getConversations(string MyName)
        {
            // Load logging file
            if (File.Exists("ConversationsAs_" + MyName + ".json"))
            {
                System.Diagnostics.Debug.WriteLine("Loading existing logs");
                string json = File.ReadAllText("ConversationsAs_" + MyName + ".json");
                Conversations = JsonSerializer.Deserialize<List<Conversation>>(json);

                System.Diagnostics.Debug.WriteLine("Returning list: ");
                System.Diagnostics.Debug.WriteLine(Conversations);
                System.Diagnostics.Debug.WriteLine("First element: ");
                System.Diagnostics.Debug.WriteLine(Conversations[0]);
                return Conversations;
            }

            return new List<Conversation>();
        }
    }
}