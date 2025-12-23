using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MccSoft.TemplateApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DbFile_Metadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "Files",
                type: "jsonb",
                nullable: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Metadata", table: "Files");
        }
    }
}
