using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace MccSoft.TemplateApp.Persistence.Migrations
{
    public partial class AuditLog_Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table =>
                    new
                    {
                        Id = table.Column<int>(type: "integer", nullable: false)
                            .Annotation(
                                "Npgsql:ValueGenerationStrategy",
                                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                            ),
                        ChangeDate = table.Column<DateTime>(
                            type: "timestamp without time zone",
                            nullable: false
                        ),
                        EntityType = table.Column<string>(type: "text", nullable: true),
                        Action = table.Column<string>(type: "text", nullable: true),
                        Key = table.Column<string>(type: "text", nullable: true),
                        FullKey = table.Column<object>(type: "jsonb", nullable: true),
                        Actual = table.Column<object>(type: "jsonb", nullable: true),
                        Change = table.Column<object>(type: "jsonb", nullable: true),
                        Old = table.Column<object>(type: "jsonb", nullable: true)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                }
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AuditLogs");
        }
    }
}
