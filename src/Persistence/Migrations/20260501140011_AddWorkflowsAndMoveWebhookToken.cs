using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChatAgentic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowsAndMoveWebhookToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_workspaces_webhook_token",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "webhook_token",
                table: "workspaces");

            migrationBuilder.CreateTable(
                name: "workflows",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    webhook_token = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    workspace_id = table.Column<int>(type: "integer", nullable: false),
                    metadata = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_workflows", x => x.id);
                    table.ForeignKey(
                        name: "fk_workflows_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_workflows_webhook_token",
                table: "workflows",
                column: "webhook_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_workflows_workspace_id",
                table: "workflows",
                column: "workspace_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workflows");

            migrationBuilder.AddColumn<string>(
                name: "webhook_token",
                table: "workspaces",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_webhook_token",
                table: "workspaces",
                column: "webhook_token");
        }
    }
}
