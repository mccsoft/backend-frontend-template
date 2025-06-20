using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MccSoft.TemplateApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookSignature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SignatureSecret",
                schema: "webhooks",
                table: "WebHookSubscriptions",
                type: "text",
                nullable: false,
                defaultValue: ""
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SignatureSecret",
                schema: "webhooks",
                table: "WebHookSubscriptions"
            );
        }
    }
}
