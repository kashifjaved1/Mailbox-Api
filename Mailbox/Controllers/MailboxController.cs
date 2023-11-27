using MailboxApi.Data;
using MailboxApi.Data.Entities;
using MailboxApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace MailboxApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailboxController : ControllerBase
    {
        private readonly ApiDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IOptions<DomainsOptions> _domainsOptions;
        private static readonly object _lock = new object();

        public MailboxController(ApiDbContext context, IConfiguration configuration, IOptions<DomainsOptions> domainOptions)
        {
            _context = context;
            _configuration = configuration;
            _domainsOptions = domainOptions;
        }

        [HttpPost("/api/create")]
        public ActionResult CreateMailbox([FromBody] MailboxCreateRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { error = "Email is required" });
            }

            var mailboxValidMail = request.Email.Split('@').LastOrDefault();
            if (!_domainsOptions.Value.AllowedDomains.Contains(mailboxValidMail))
            {
                return BadRequest("Invalid email domain");
            }

            var newMailbox = new Mailbox { Email = request.Email, Username = request.Username };

            try
            {
                // Add mailbox to the database
                _context.Mailboxes.Add(newMailbox);
                _context.SaveChanges();
                
                return CreatedAtAction(nameof(CreateMailbox), new { message = "Mailbox created successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { error = $"Error creating mailbox: {e.Message}" });
            }
        }

        [HttpPut("/api/update")]
        public ActionResult UpdateMailbox(int id, [FromBody] MailboxUpdateRequest request)
        {
            var existingMailbox = _context.Mailboxes.Find(id);

            if (existingMailbox == null)
            {
                return NotFound(new { error = "Mailbox not found" });
            }

            existingMailbox.Username = request.Username;

            try
            {
                _context.SaveChanges();
                return Ok(new { message = "Mailbox updated successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { error = $"Error updating mailbox: {e.Message}" });
            }
        }

        [HttpDelete("/api/delete/{id}")]
        public ActionResult DeleteMailbox(int id)
        {
            var existingMailbox = _context.Mailboxes.Find(id);

            if (existingMailbox == null)
            {
                return NotFound(new { error = "Mailbox not found" });
            }

            try
            {
                _context.Mailboxes.Remove(existingMailbox);
                _context.SaveChanges();
                return Ok(new { message = "Mailbox deleted successfully" });
            }
            catch (Exception e)
            {
                return StatusCode(500, new { error = $"Error deleting mailbox: {e.Message}" });
            }
        }

        [HttpPost("/api/upgrade_plan/{email}")]
        public async Task<ActionResult> UpgradePlan(string email)
        {
            var userMailbox = _context.Mailboxes.SingleOrDefault(m => m.Email == email);

            if (userMailbox == null)
            {
                return NotFound(new { error = "Mailbox not found" });
            }

            // Prepare PayPal API request to create an order
            var order = await CreatePayPalOrder(userMailbox.Id, 10.0); // Assuming the upgrade cost is $10

            // If the order creation is successful, redirect the user to the PayPal approval URL
            dynamic linksDynamic = order.Links;
            var approveLink = ((IEnumerable<dynamic>)linksDynamic).FirstOrDefault(link => link.Rel == "approve");
            if (approveLink != null)
            {
                return Redirect(approveLink.Href);
            }

            return BadRequest(new { error = "Failed to create PayPal order" });
        }

        private async Task<dynamic> CreatePayPalOrder(int userId, double upgradeCost)
        {
            var PayPalClientId = _configuration["Paypal:ClientId"];
            var PayPalClientSecret = _configuration["Paypal:ClientSecret"];

            using (var httpClient = new HttpClient())
            {
                var credentials = $"{PayPalClientId}:{PayPalClientSecret}";
                var credentialsBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentialsBase64);

                var requestBody = new Dictionary<string, string>
                {
                    { "intent", "CAPTURE" },
                    { "purchase_units[0].amount.value", upgradeCost.ToString() },
                    { "purchase_units[0].amount.currency_code", "USD" },
                    { "application_context.return_url", $"https://your-app.com/upgrade-success/{userId}" }, // $"{Request.Scheme}://{Request.Host}/upgrade-success/{userId}";
                    { "application_context.cancel_url", $"https://your-app.com/upgrade-cancel" }, // $"{Request.Scheme}://{Request.Host}/upgrade-cancel";
                };

                var requestContent = new FormUrlEncodedContent(requestBody);

                var response = await httpClient.PostAsync("https://api.sandbox.paypal.com/v2/checkout/orders", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<dynamic>(responseContent);
                }

                return null;
            }
        }
    }
}
