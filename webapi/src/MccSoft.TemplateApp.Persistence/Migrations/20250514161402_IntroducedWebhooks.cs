using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MccSoft.TemplateApp.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IntroducedWebhooks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "WebHook");

            migrationBuilder.EnsureSchema(name: "webhooks");

            migrationBuilder.CreateTable(
                name: "WebHookSubscriptions",
                schema: "webhooks",
                columns: table =>
                    new
                    {
                        Id = table.Column<Guid>(type: "uuid", nullable: false),
                        TenantId = table.Column<int>(type: "integer", nullable: false),
                        Name = table.Column<string>(type: "text", nullable: false),
                        Url = table.Column<string>(type: "text", nullable: false),
                        Method = table.Column<string>(type: "text", nullable: false),
                        EventType = table.Column<string>(type: "text", nullable: false),
                        Headers = table.Column<string>(type: "text", nullable: false),
                        SubscribedAt = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: false
                        )
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHookSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebHookSubscriptions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateTable(
                name: "WebHooks",
                schema: "webhooks",
                columns: table =>
                    new
                    {
                        Id = table
                            .Column<int>(type: "integer", nullable: false)
                            .Annotation(
                                "Npgsql:ValueGenerationStrategy",
                                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                            ),
                        CreatedAt = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: false
                        ),
                        IsSucceeded = table.Column<bool>(type: "boolean", nullable: false),
                        IsFinished = table.Column<bool>(type: "boolean", nullable: false),
                        StatusCode = table.Column<int>(type: "integer", nullable: true),
                        LastError = table.Column<string>(type: "text", nullable: true),
                        Response = table.Column<string>(type: "text", nullable: true),
                        EventType = table.Column<string>(type: "text", nullable: false),
                        Data = table.Column<string>(type: "text", nullable: false),
                        AttemptsPerformed = table.Column<int>(type: "integer", nullable: false),
                        LastAttempt = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: true
                        ),
                        SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WebHooks_WebHookSubscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalSchema: "webhooks",
                        principalTable: "WebHookSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade
                    );
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_WebHooks_SubscriptionId",
                schema: "webhooks",
                table: "WebHooks",
                column: "SubscriptionId"
            );

            migrationBuilder.CreateIndex(
                name: "IX_WebHookSubscriptions_TenantId",
                schema: "webhooks",
                table: "WebHookSubscriptions",
                column: "TenantId"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "WebHooks", schema: "webhooks");

            migrationBuilder.DropTable(name: "WebHookSubscriptions", schema: "webhooks");

            migrationBuilder.CreateTable(
                name: "WebHook",
                columns: table =>
                    new
                    {
                        Id = table
                            .Column<int>(type: "integer", nullable: false)
                            .Annotation(
                                "Npgsql:ValueGenerationStrategy",
                                NpgsqlValueGenerationStrategy.IdentityByDefaultColumn
                            ),
                        AttemptsPerformed = table.Column<int>(type: "integer", nullable: false),
                        CreatedAt = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: false
                        ),
                        Headers = table.Column<string>(type: "text", nullable: false),
                        IsFinished = table.Column<bool>(type: "boolean", nullable: false),
                        IsSucceeded = table.Column<bool>(type: "boolean", nullable: false),
                        LastError = table.Column<string>(type: "text", nullable: true),
                        LastRun = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: true
                        ),
                        NextRun = table.Column<DateTime>(
                            type: "timestamp with time zone",
                            nullable: true
                        ),
                        Response = table.Column<string>(type: "text", nullable: true),
                        AdditionalData_Int = table.Column<int>(type: "integer", nullable: true),
                        AdditionalData_Json = table.Column<JsonDocument>(
                            type: "jsonb",
                            nullable: true
                        ),
                        AdditionalData_String = table.Column<string>(type: "text", nullable: true)
                    },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHook", x => x.Id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "IX_WebHook_AdditionalData_Int",
                table: "WebHook",
                column: "AdditionalData_Int"
            );

            migrationBuilder.CreateIndex(
                name: "IX_WebHook_AdditionalData_String",
                table: "WebHook",
                column: "AdditionalData_String"
            );

            migrationBuilder.CreateIndex(
                name: "IX_WebHook_IsSucceeded_NextRun",
                table: "WebHook",
                columns: new[] { "IsSucceeded", "NextRun" }
            );
        }
    }
}
