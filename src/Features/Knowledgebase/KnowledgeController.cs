using ChatAgentic.Features.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ChatAgentic.Entities;

namespace ChatAgentic.Features.Knowledgebase
{
    [ApiController]
    [Route("knowledge")]
    [Authorize]
    public class KnowledgeController : ControllerBase
    {
        public KnowledgeController()
        {
        }

        private int WorkspaceId => User.GetWorkspaceId();

        [HttpPost("ingestion")]
        public async Task<IActionResult> Ingest([FromForm] KnowledgeIngestionDTO dto, [FromServices]KnowledgeBaseIngestor ingestor, CancellationToken ct)
        {
            if (dto.File == null)
                return BadRequest(new { Message = "File is required" });

            var token = dto.Token ?? string.Empty;
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { Message = "Token is required" });

            await ingestor.ExecuteAsync(new KnowledgeBaseIngestorInput(
                WorkspaceId: WorkspaceId,
                Context: dto.Context ?? Knowledge.DefaultContext,
                ChunkerType: dto.ChunkerType,
                ClearText: dto.ClearText ?? false,
                Filename: dto.File.FileName,
                File: dto.File.OpenReadStream()
            ));

            return Ok(null);
        }
    }
}
