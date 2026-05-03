using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChatAgentic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameWorkflowToAgents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_conversations_workflows_workflow_id",
                table: "conversations");

            migrationBuilder.RenameColumn(
                name: "workflow_id",
                table: "conversations",
                newName: "agent_id");

            migrationBuilder.RenameIndex(
                name: "ix_conversations_workspace_id_workflow_id_channel_sender_ident",
                table: "conversations",
                newName: "ix_conversations_workspace_id_agent_id_channel_sender_identifi");

            migrationBuilder.RenameIndex(
                name: "ix_conversations_workflow_id",
                table: "conversations",
                newName: "ix_conversations_agent_id");

            migrationBuilder.CreateTable(
                name: "agents",
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
                    table.PrimaryKey("pk_agents", x => x.id);
                    table.ForeignKey(
                        name: "fk_agents_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql("INSERT INTO agents (id, name, webhook_token, workspace_id, metadata) SELECT id, name, webhook_token, workspace_id, metadata FROM workflows");

            migrationBuilder.DropTable(
                name: "workflows");

            migrationBuilder.CreateIndex(
                name: "ix_agents_webhook_token",
                table: "agents",
                column: "webhook_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_agents_workspace_id",
                table: "agents",
                column: "workspace_id");

            migrationBuilder.AddForeignKey(
                name: "fk_conversations_agents_agent_id",
                table: "conversations",
                column: "agent_id",
                principalTable: "agents",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_conversations_agents_agent_id",
                table: "conversations");

            migrationBuilder.DropTable(
                name: "agents");

            migrationBuilder.RenameColumn(
                name: "agent_id",
                table: "conversations",
                newName: "workflow_id");

            migrationBuilder.RenameIndex(
                name: "ix_conversations_workspace_id_agent_id_channel_sender_identifi",
                table: "conversations",
                newName: "ix_conversations_workspace_id_workflow_id_channel_sender_ident");

            migrationBuilder.RenameIndex(
                name: "ix_conversations_agent_id",
                table: "conversations",
                newName: "ix_conversations_workflow_id");

            migrationBuilder.CreateTable(
                name: "workflows",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    workspace_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    webhook_token = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
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

            migrationBuilder.AddForeignKey(
                name: "fk_conversations_workflows_workflow_id",
                table: "conversations",
                column: "workflow_id",
                principalTable: "workflows",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
