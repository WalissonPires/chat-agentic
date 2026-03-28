using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatAgentic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceIntegrationToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "integration_token",
                table: "workspaces",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_workspaces_integration_token",
                table: "workspaces",
                column: "integration_token");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_workspaces_integration_token",
                table: "workspaces");

            migrationBuilder.DropColumn(
                name: "integration_token",
                table: "workspaces");
        }
    }
}
