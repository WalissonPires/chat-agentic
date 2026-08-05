using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ChatAgentic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_rules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    channels = table.Column<int[]>(type: "integer[]", nullable: false),
                    message_template = table.Column<string>(type: "text", nullable: false),
                    frequency = table.Column<string>(type: "text", nullable: false),
                    send_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    send_day = table.Column<int>(type: "integer", nullable: true),
                    send_month = table.Column<int>(type: "integer", nullable: true),
                    target_type = table.Column<string>(type: "text", nullable: false),
                    target_person_ids = table.Column<List<int>>(type: "integer[]", nullable: false),
                    enabled = table.Column<bool>(type: "boolean", nullable: false),
                    last_executed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    next_execution_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    hangfire_job_id = table.Column<string>(type: "text", nullable: true),
                    workspace_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    target_filters = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_rules", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_rules_workspaces_workspace_id",
                        column: x => x.workspace_id,
                        principalTable: "workspaces",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notification_logs",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    notification_rule_id = table.Column<int>(type: "integer", nullable: false),
                    person_id = table.Column<int>(type: "integer", nullable: false),
                    channel = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    execution_batch_id = table.Column<Guid>(type: "uuid", nullable: false),
                    execution_period_key = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_logs", x => x.id);
                    table.ForeignKey(
                        name: "fk_notification_logs_notification_rules_notification_rule_id",
                        column: x => x.notification_rule_id,
                        principalTable: "notification_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_notification_logs_people_person_id",
                        column: x => x.person_id,
                        principalTable: "people",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notification_logs_notification_rule_id_execution_batch_id",
                table: "notification_logs",
                columns: new[] { "notification_rule_id", "execution_batch_id" });

            migrationBuilder.CreateIndex(
                name: "ix_notification_logs_notification_rule_id_person_id_execution_",
                table: "notification_logs",
                columns: new[] { "notification_rule_id", "person_id", "execution_period_key" },
                unique: true,
                filter: "status = 'Sent'");

            migrationBuilder.CreateIndex(
                name: "ix_notification_logs_notification_rule_id_sent_at",
                table: "notification_logs",
                columns: new[] { "notification_rule_id", "sent_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notification_logs_person_id",
                table: "notification_logs",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "ix_notification_rules_enabled_next_execution_at",
                table: "notification_rules",
                columns: new[] { "enabled", "next_execution_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notification_rules_workspace_id",
                table: "notification_rules",
                column: "workspace_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_logs");

            migrationBuilder.DropTable(
                name: "notification_rules");
        }
    }
}
