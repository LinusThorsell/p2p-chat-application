using System;

namespace TDDD49Template.Models
{
    public class MessagePacket
    {
        public string? RequestType { get; set; }
        public string? Name { get; set; }
        public DateTime Date { get; set; }
        public string? Message { get; set; }
    }
}
