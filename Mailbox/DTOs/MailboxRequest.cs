namespace MailboxApi.DTOs
{
    public class MailboxCreateRequest
    {
        public string Email { get; set; }
        public string Username { get; set; }
    }

    public class MailboxUpdateRequest
    {
        public string Username { get; set; }
    }
}
