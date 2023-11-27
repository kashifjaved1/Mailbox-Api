using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MailboxApi.Migrations
{
    /// <inheritdoc />
    public partial class initdb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mailboxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    StorageCapacityMb = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mailboxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SenderEmail = table.Column<string>(type: "text", nullable: false),
                    RecipientEmail = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Display = table.Column<bool>(type: "boolean", nullable: false),
                    MailboxId = table.Column<int>(type: "integer", nullable: true),
                    MailboxId1 = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Mailboxes_MailboxId",
                        column: x => x.MailboxId,
                        principalTable: "Mailboxes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Messages_Mailboxes_MailboxId1",
                        column: x => x.MailboxId1,
                        principalTable: "Mailboxes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mailboxes_Email",
                table: "Mailboxes",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MailboxId",
                table: "Messages",
                column: "MailboxId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_MailboxId1",
                table: "Messages",
                column: "MailboxId1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Mailboxes");
        }
    }
}
