using MailboxApi.Data;
using MailboxApi.Data.Entities;
using MailboxApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MailboxApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public MailController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpPost("/api/send")]
        public ActionResult SendMessage([FromBody] MessageRequest request)
        {
            if (string.IsNullOrEmpty(request.SenderEmail) || string.IsNullOrEmpty(request.RecipientEmail) || string.IsNullOrEmpty(request.Content))
            {
                return BadRequest(new { error = "Sender username, recipient username, and content are required" });
            }

            var senderMailbox = _context.Mailboxes.SingleOrDefault(m => m.Email == request.SenderEmail);
            var recipientMailbox = _context.Mailboxes.SingleOrDefault(m => m.Email == request.RecipientEmail);

            if (senderMailbox == null || recipientMailbox == null)
            {
                return NotFound(new { error = "Sender or recipient not found" });
            }

            var newMessage = new Message
            {
                SenderEmail = senderMailbox.Email,
                RecipientEmail = recipientMailbox.Email,
                Content = request.Content,
                Display = true
            };

            try
            {
                // Check sender's storage capacity before sending the message
                if (senderMailbox.StorageCapacityMb + newMessage.SizeMb > 1.0)
                {
                    return BadRequest(new { error = "Sender's storage capacity exceeded. Cannot send the message." });
                }

                _context.Messages.Add(newMessage);
                _context.SaveChanges();

                // Update sender and recipient storage capacity
                UpdateStorageCapacity(senderMailbox);
                UpdateStorageCapacity(recipientMailbox);

                return CreatedAtAction(nameof(SendMessage), new { message = "Message sent successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { error = $"Error sending message: {e.Message}" });
            }
        }

        [HttpGet("/api/read/{email}")]
        public ActionResult ReadMessages(string email)
        {
            var userMailbox = _context.Mailboxes.SingleOrDefault(m => m.Email == email);

            if (userMailbox == null)
            {
                return NotFound(new { error = "Mailbox not found" });
            }

            // Retrieve only messages with Display=true
            var visibleMessages = _context.Messages
                .Where(m => m.RecipientEmail == userMailbox.Email && m.Display)
                .ToList();

            return Ok(visibleMessages);
        }

        private void UpdateStorageCapacity(Mailbox mailbox)
        {
            double totalSizeMb = (mailbox.SentMessages?.Sum(m => m.SizeMb) ?? 0) + (mailbox.ReceivedMessages?.Sum(m => m.SizeMb) ?? 0);
            mailbox.StorageCapacityMb = totalSizeMb;

            // If the storage capacity exceeds 1MB, set Display=false for received messages
            if (totalSizeMb > 1.0)
            {
                foreach (var receivedMessage in mailbox.ReceivedMessages)
                {
                    receivedMessage.Display = false;
                }

                // Issue a warning or handle as needed
                Console.WriteLine($"Warning: Mailbox '{mailbox.Email}' has exceeded 1MB storage capacity. Display=false for received messages.");
            }
        }
    }
}
