using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChatAgentic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExtractChannelEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "telegram_channel_id",
                table: "agents",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "whatsapp_channel_id",
                table: "agents",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "channels",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    workspace_id = table.Column<int>(type: "integer", nullable: false),
                    credentials = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_channels", x => x.id);
                    table.ForeignKey(
                        name: "fk_channels_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_agents_telegram_channel_id",
                table: "agents",
                column: "telegram_channel_id");

            migrationBuilder.CreateIndex(
                name: "ix_agents_whatsapp_channel_id",
                table: "agents",
                column: "whatsapp_channel_id");

            migrationBuilder.CreateIndex(
                name: "ix_channels_workspace_id",
                table: "channels",
                column: "workspace_id");

            migrationBuilder.AddForeignKey(
                name: "fk_agents_channels_telegram_channel_id",
                table: "agents",
                column: "telegram_channel_id",
                principalTable: "channels",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_agents_channels_whatsapp_channel_id",
                table: "agents",
                column: "whatsapp_channel_id",
                principalTable: "channels",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_agents_channels_telegram_channel_id",
                table: "agents");

            migrationBuilder.DropForeignKey(
                name: "fk_agents_channels_whatsapp_channel_id",
                table: "agents");

            migrationBuilder.DropTable(
                name: "channels");

            migrationBuilder.DropIndex(
                name: "ix_agents_telegram_channel_id",
                table: "agents");

            migrationBuilder.DropIndex(
                name: "ix_agents_whatsapp_channel_id",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "telegram_channel_id",
                table: "agents");

            migrationBuilder.DropColumn(
                name: "whatsapp_channel_id",
                table: "agents");
        }
    }
}
