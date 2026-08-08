using ChatAgentic.Features.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatAgentic.Features.Notifications
{
    [ApiController]
    [Route("notifications")]
    [Authorize]
    public class NotificationRulesController : ControllerBase
    {
        private readonly INotificationRuleService _ruleService;
        private readonly ICurrentUser _currentUser;

        public NotificationRulesController(INotificationRuleService ruleService, ICurrentUser currentUser)
        {
            _ruleService = ruleService;
            _currentUser = currentUser;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] NotificationRuleInput input, CancellationToken ct)
        {
            var result = await _ruleService.CreateAsync(_currentUser.WorkspaceId, input, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet]
        public async Task<ActionResult<List<NotificationRuleOutput>>> List(CancellationToken ct)
        {
            var results = await _ruleService.ListAsync(_currentUser.WorkspaceId, ct);
            return Ok(results);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<NotificationRuleOutput>> GetById(int id, CancellationToken ct)
        {
            var result = await _ruleService.GetByIdAsync(_currentUser.WorkspaceId, id, ct);
            if (result == null)
                return NotFound(new { Message = $"Notification rule {id} not found." });

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<NotificationRuleOutput>> Update(int id, [FromBody] NotificationRuleInput input, CancellationToken ct)
        {
            var result = await _ruleService.UpdateAsync(_currentUser.WorkspaceId, id, input, ct);
            if (result == null)
                return NotFound(new { Message = $"Notification rule {id} not found." });

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id, CancellationToken ct)
        {
            var deleted = await _ruleService.DeleteAsync(_currentUser.WorkspaceId, id, ct);
            if (!deleted)
                return NotFound(new { Message = $"Notification rule {id} not found." });

            return NoContent();
        }

        [HttpPost("{id:int}/trigger")]
        public async Task<ActionResult<TriggerOutput>> Trigger(int id, CancellationToken ct)
        {
            var batchId = await _ruleService.TriggerAsync(_currentUser.WorkspaceId, id, ct);
            if (batchId == null)
                return NotFound(new { Message = $"Notification rule {id} not found." });

            return Ok(new TriggerOutput("Notification rule triggered successfully", batchId.Value));
        }
    }

    public record TriggerOutput(
        string Message,
        Guid ExecutionBatchId
    );
}
