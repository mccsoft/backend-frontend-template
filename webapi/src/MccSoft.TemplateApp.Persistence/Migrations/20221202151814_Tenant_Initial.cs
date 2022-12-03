using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MccSoft.TemplateApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TenantInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("TRUNCATE \"Products\"");
            migrationBuilder.Sql("DELETE FROM \"AuditLogs\"");
            migrationBuilder.Sql("DELETE FROM \"AspNetUserLogins\"");
            migrationBuilder.Sql("DELETE FROM \"AspNetUserClaims\"");
            migrationBuilder.Sql("DELETE FROM \"AspNetUsers\"");

            migrationBuilder.DropColumn(name: "OwnerId", table: "Products");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Products",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0
            );

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table =>
                    new
                    {
                        Id = table
                            .Column<int>(type: "integer", nullable: false)
                            .Annotation(
                                "Npgsql:ValueGenerationStrategy",
                                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                            )
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers",
                column: "TenantId"
            );

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Tenants_TenantId",
                table: "AspNetUsers"
            );

            migrationBuilder.DropTable(name: "Tenants");

            migrationBuilder.DropIndex(name: "IX_AspNetUsers_TenantId", table: "AspNetUsers");

            migrationBuilder.DropColumn(name: "TenantId", table: "Products");

            migrationBuilder.DropColumn(name: "TenantId", table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "OwnerId",
                table: "Products",
                type: "text",
                nullable: false,
                defaultValue: ""
            );
        }
    }
}
