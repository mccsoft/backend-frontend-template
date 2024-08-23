using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MccSoft.TemplateApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DbFile_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table =>
                    new
                    {
                        Id = table.Column<Guid>(type: "uuid", nullable: false),
                        FileName = table.Column<string>(
                            type: "character varying(256)",
                            maxLength: 256,
                            nullable: false
                        ),
                        PathOnDisk = table.Column<string>(type: "text", nullable: false),
                        Hash = table.Column<byte[]>(type: "bytea", nullable: false),
                        Size = table.Column<long>(type: "bigint", nullable: false),
                        CreatedAt = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: false
                        )
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.Id);
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Files");
        }
    }
}
