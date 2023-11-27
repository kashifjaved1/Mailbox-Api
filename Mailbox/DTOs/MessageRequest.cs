namespace MailboxApi.DTOs
{
    public class MessageRequest
    {
        public string SenderEmail { get; set; }
        public string RecipientEmail { get; set; }
        public string Content { get; set; }
    }
}
