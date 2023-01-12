using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MccSoft.TemplateApp.Persistence.Migrations
{
    public partial class WebHooks_Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebHook",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastRun = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextRun = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsSucceeded = table.Column<bool>(type: "boolean", nullable: false),
                    IsFinished = table.Column<bool>(type: "boolean", nullable: false),
                    LastError = table.Column<string>(type: "text", nullable: true),
                    Response = table.Column<string>(type: "text", nullable: true),
                    AttemptsPerformed = table.Column<int>(type: "integer", nullable: false),
                    Headers = table.Column<string>(type: "text", nullable: false),
                    AdditionalData_Int = table.Column<int>(type: "integer", nullable: true),
                    AdditionalData_String = table.Column<string>(type: "text", nullable: true),
                    AdditionalData_Json = table.Column<JsonDocument>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHook", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WebHook_AdditionalData_Int",
                table: "WebHook",
                column: "AdditionalData_Int");

            migrationBuilder.CreateIndex(
                name: "IX_WebHook_AdditionalData_String",
                table: "WebHook",
                column: "AdditionalData_String");

            migrationBuilder.CreateIndex(
                name: "IX_WebHook_IsSucceeded_NextRun",
                table: "WebHook",
                columns: new[] { "IsSucceeded", "NextRun" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebHook");
        }
    }
}
