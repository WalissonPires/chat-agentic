using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChatAgentic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAIUsageHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ai_usage_histories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    workspace_id = table.Column<int>(type: "integer", nullable: false),
                    conversation_id = table.Column<int>(type: "integer", nullable: true),
                    provider = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    service = table.Column<string>(type: "text", nullable: false),
                    input = table.Column<long>(type: "bigint", nullable: false),
                    output = table.Column<long>(type: "bigint", nullable: false),
                    cost = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ai_usage_histories", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ai_usage_histories_conversation_id",
                table: "ai_usage_histories",
                column: "conversation_id");

            migrationBuilder.CreateIndex(
                name: "ix_ai_usage_histories_provider_service",
                table: "ai_usage_histories",
                columns: new[] { "provider", "service" });

            migrationBuilder.CreateIndex(
                name: "ix_ai_usage_histories_workspace_id_created_at",
                table: "ai_usage_histories",
                columns: new[] { "workspace_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ai_usage_histories");
        }
    }
}
