using MailboxApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MailboxApi.Data
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options)
        {
        }

        public DbSet<Mailbox> Mailboxes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Mailbox>()
                .HasIndex(m => m.Email)
                .IsUnique();
        }
    }
}
