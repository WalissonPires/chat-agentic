using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatAgentic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowIdToConversations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_conversations_workspace_id_channel_sender_identifier",
                table: "conversations");

            migrationBuilder.AddColumn<int>(
                name: "workflow_id",
                table: "conversations",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "ix_conversations_workflow_id",
                table: "conversations",
                column: "workflow_id");

            migrationBuilder.CreateIndex(
                name: "ix_conversations_workspace_id_workflow_id_channel_sender_ident",
                table: "conversations",
                columns: new[] { "workspace_id", "workflow_id", "channel", "sender_identifier" });

            migrationBuilder.AddForeignKey(
                name: "fk_conversations_workflows_workflow_id",
                table: "conversations",
                column: "workflow_id",
                principalTable: "workflows",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_conversations_workflows_workflow_id",
                table: "conversations");

            migrationBuilder.DropIndex(
                name: "ix_conversations_workflow_id",
                table: "conversations");

            migrationBuilder.DropIndex(
                name: "ix_conversations_workspace_id_workflow_id_channel_sender_ident",
                table: "conversations");

            migrationBuilder.DropColumn(
                name: "workflow_id",
                table: "conversations");

            migrationBuilder.CreateIndex(
                name: "ix_conversations_workspace_id_channel_sender_identifier",
                table: "conversations",
                columns: new[] { "workspace_id", "channel", "sender_identifier" });
        }
    }
}
