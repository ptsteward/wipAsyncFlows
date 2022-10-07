using System.Threading.Tasks.Dataflow;

namespace AsyncFlows.AsyncMediator;

internal sealed class FlowSource<TItem>
    : IFlowSource<TItem>,
    ILinkableSource<TItem>
    where TItem : Envelope
{
    private volatile bool isDisposed;
    private BroadcastBlock<TItem>? Source { get; set; }

    public FlowSource()
        => Source = new(item => item,
            new()
            {
                EnsureOrdered = true,
                BoundedCapacity = DataflowBlockOptions.Unbounded
            });

    public ValueTask<bool> EmitAsync(TItem item, CancellationToken cancelToken = default)
        => Source.ThrowIfDisposed(isDisposed)
            .SendAsync(item, cancelToken);

    public IDisposable LinkTo(ITargetBlock<TItem> sink)
        => Source.ThrowIfDisposed(isDisposed)
            .LinkTo(sink, new()
            {
                PropagateCompletion = true,
            });

    /// <summary>
    /// This resource is intended for async disposal. Please use 
    /// <code>await using</code>
    /// </summary>
    public void Dispose()
    {
        if (isDisposed) return;
        DisposeAsyncCore().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (isDisposed) return;
        await DisposeAsyncCore();
    }

    private async ValueTask DisposeAsyncCore()
    {
        isDisposed = true;
        Source?.Complete();
        await (Source?.Completion ?? Task.CompletedTask);
        Source = null;
        GC.SuppressFinalize(this);
    }
}
