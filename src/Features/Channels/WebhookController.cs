using System.Text.Json;
using ChatAgentic.Features.Channels;
using ChatAgentic.Features.Workspaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatAgentic.Features.Knowledgebase
{
    [ApiController]
    [Route("webhook")]
    [Authorize]
    public class WebhookController : ControllerBase
    {
        public WebhookController()
        {
        }

        private int WorkspaceId => User.GetWorkspaceId();

        [HttpPost("{channel}/{token}")]
        public async Task<IActionResult> ReceiveMessage([FromRoute] string channel, [FromRoute] string token, [FromBody] JsonElement body,
            [FromServices] WebhookMessageProcessor messageProcessor, CancellationToken ct)
        {
            var channelType = Enum.Parse<ChannelType>(channel, ignoreCase: true);
            await messageProcessor.Execute(new(channelType, token, body.ToString()));
            return Ok(null);
        }
    }
}
