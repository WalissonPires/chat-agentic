using ChatAgentic.Entities;
using ChatAgentic.Features.Channels;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Persistence
{
    public sealed class ChannelLoader
    {
        private readonly AppDbContext _db;
        private readonly ChannelContext _channelContext;

        public ChannelLoader(AppDbContext db, ChannelContext channelContext)
        {
            _db = db;
            _channelContext = channelContext;
        }

        public async Task<Channel?> LoadByIdAsync(int channelId, CancellationToken ct = default)
        {
            var channel = await _db.Channels.AsNoTracking().FirstOrDefaultAsync(c => c.Id == channelId, ct);
            if (channel != null)
            {
                _channelContext.SetFromChannel(channel);
            }
            return channel;
        }

        public async Task<Channel?> LoadByWorkspaceAndTypeAsync(int workspaceId, ChannelType type, CancellationToken ct = default)
        {
            var channel = await _db.Channels.AsNoTracking().FirstOrDefaultAsync(c => c.WorkspaceId == workspaceId && c.Type == type, ct);
            if (channel != null)
            {
                _channelContext.SetFromChannel(channel);
            }
            return channel;
        }
    }
}
