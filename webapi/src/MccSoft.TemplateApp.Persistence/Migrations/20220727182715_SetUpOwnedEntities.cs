using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MccSoft.TemplateApp.Persistence.Migrations
{
    public partial class SetUpOwnedEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Products",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Products");
        }
    }
}
