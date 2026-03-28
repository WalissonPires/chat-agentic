using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace ChatAgentic.Queue;

public sealed class InMemoryMessageQueue<T> : IMessageQueue<T>
{
    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>(new UnboundedChannelOptions { SingleReader = true });

    public ValueTask EnqueueAsync(T item, CancellationToken ct = default)
    {
        return _channel.Writer.WriteAsync(item, ct);
    }

    public async IAsyncEnumerable<T> DequeueAllAsync([EnumeratorCancellation] CancellationToken ct)
    {
        await foreach (var item in _channel.Reader.ReadAllAsync(ct))
            yield return item;
    }
}
