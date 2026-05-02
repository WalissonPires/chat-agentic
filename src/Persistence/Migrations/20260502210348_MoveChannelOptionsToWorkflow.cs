using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatAgentic.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MoveChannelOptionsToWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Os blocos EvolutionApi/Telegram sao OwnsOne aninhados em jsonb. Copiamos do
            // workspaces.metadata para o workflows.metadata de cada workflow do mesmo workspace,
            // e depois removemos as chaves do workspace.
            // Em ambiente sem dados pre-existentes, ambos os UPDATEs sao no-op.
            migrationBuilder.Sql(@"
                UPDATE workflows w
                SET metadata = COALESCE(w.metadata, '{}'::jsonb)
                    || jsonb_strip_nulls(jsonb_build_object(
                        'EvolutionApi', ws.metadata -> 'EvolutionApi',
                        'Telegram',     ws.metadata -> 'Telegram'))
                FROM workspaces ws
                WHERE w.workspace_id = ws.id
                  AND ws.metadata IS NOT NULL
                  AND (ws.metadata ? 'EvolutionApi' OR ws.metadata ? 'Telegram');
            ");

            migrationBuilder.Sql(@"
                UPDATE workspaces
                SET metadata = metadata - 'EvolutionApi' - 'Telegram'
                WHERE metadata IS NOT NULL
                  AND (metadata ? 'EvolutionApi' OR metadata ? 'Telegram');
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverte: copia EvolutionApi/Telegram do workflow de volta para o workspace
            // (usa o ultimo workflow encontrado por workspace) e remove dos workflows.
            migrationBuilder.Sql(@"
                UPDATE workspaces ws
                SET metadata = COALESCE(ws.metadata, '{}'::jsonb)
                    || jsonb_strip_nulls(jsonb_build_object(
                        'EvolutionApi', w.metadata -> 'EvolutionApi',
                        'Telegram',     w.metadata -> 'Telegram'))
                FROM workflows w
                WHERE w.workspace_id = ws.id
                  AND w.metadata IS NOT NULL
                  AND (w.metadata ? 'EvolutionApi' OR w.metadata ? 'Telegram');
            ");

            migrationBuilder.Sql(@"
                UPDATE workflows
                SET metadata = metadata - 'EvolutionApi' - 'Telegram'
                WHERE metadata IS NOT NULL
                  AND (metadata ? 'EvolutionApi' OR metadata ? 'Telegram');
            ");
        }
    }
}
