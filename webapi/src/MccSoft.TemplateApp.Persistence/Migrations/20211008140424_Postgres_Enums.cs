using MccSoft.TemplateApp.Domain;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MccSoft.TemplateApp.Persistence.Migrations
{
    public partial class Postgres_Enums : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"TRUNCATE TABLE ""Products""");
            migrationBuilder
                .AlterDatabase()
                .Annotation("Npgsql:Enum:product_type", "undefined,auto,electronic,other");

            migrationBuilder.AddColumn<ProductType>(
                name: "ProductType",
                table: "Products",
                type: "product_type",
                nullable: false,
                defaultValue: ProductType.Undefined
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "ProductType", table: "Products");

            migrationBuilder
                .AlterDatabase()
                .OldAnnotation("Npgsql:Enum:product_type", "undefined,auto,electronic,other");
        }
    }
}
