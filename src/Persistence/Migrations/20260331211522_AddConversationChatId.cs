using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatAgentic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConversationChatId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "chat_id",
                table: "conversations",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "chat_id",
                table: "conversations");
        }
    }
}
