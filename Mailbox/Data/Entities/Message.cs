namespace MailboxApi.Data.Entities
{
    public class Message : Entity
    {
        public required string SenderEmail { get; set; }
        public required string RecipientEmail { get; set; }
        public string Content { get; set; }
        public bool Display { get; set; }

        public double SizeMb
        {
            get { return Content.Length / 1024.0 / 1024.0; } // Assuming Content is a string
        }
    }
}
