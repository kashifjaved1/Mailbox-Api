using MailboxApi.Controllers;

namespace MailboxApi.Data.Entities
{
    public class Mailbox : Entity
    {
        public string Username { get; set; }
        public required string Email { get; set; }
        public List<Message> SentMessages { get; set; }
        public List<Message> ReceivedMessages { get; set; }
        public double StorageCapacityMb { get; set; }
    }
}
