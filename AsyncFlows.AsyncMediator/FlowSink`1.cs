using System.Threading.Tasks.Dataflow;

namespace AsyncFlows.AsyncMediator;

internal sealed class FlowSink<TItem>
    : IFlowSink<TItem>
    where TItem : Envelope
{
    private readonly IDisposable link;
    private volatile bool isDisposed;
    private BufferBlock<TItem>? Buffer { get; set; }

    public FlowSink(ILinkableSource<TItem> source)
    {
        Buffer = new(new()
        {
            EnsureOrdered = true,
            BoundedCapacity = DataflowBlockOptions.Unbounded
        });
        link = source.LinkTo(Buffer);
    }

    public IAsyncEnumerable<TItem> ConsumeAsync(CancellationToken cancelToken = default)
        => Buffer.ThrowIfDisposed(isDisposed)
            .ToAsyncEnumerable(cancelToken);

    public void Dispose()
    {
        if (isDisposed) return;
        DisposeCore();
    }

    public ValueTask DisposeAsync()
    {
        if (!isDisposed)
            DisposeCore();
        return ValueTask.CompletedTask;
    }

    private void DisposeCore()
    {
        isDisposed = true;
        link?.Dispose();
        Buffer = null;
        GC.SuppressFinalize(this);
    }
}
