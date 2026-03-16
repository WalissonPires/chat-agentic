namespace ChatAgentic.Workflows
{
    public interface IMessageQueue<T>
    {
        ValueTask EnqueueAsync(T item, CancellationToken ct = default);

        IAsyncEnumerable<T> DequeueAllAsync(CancellationToken ct);
    }
}