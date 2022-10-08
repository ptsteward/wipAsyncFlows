using System.Threading.Tasks.Dataflow;

namespace AsyncFlows.AsyncMediator.Flows;

public sealed class FlowSink<TSchema>
    : IFlowSink<TSchema>
    where TSchema : Envelope
{
    private readonly IDisposable link;
    public volatile bool isDisposed;
    public BufferBlock<TSchema>? Buffer { get; set; }

    public FlowSink(ILinkableSource<TSchema> source)
    {
        Buffer = new(new()
        {
            EnsureOrdered = true,
            BoundedCapacity = DataflowBlockOptions.Unbounded
        });
        link = source.LinkTo(Buffer);
    }

    public IAsyncEnumerable<TSchema> ConsumeAsync(CancellationToken cancelToken = default) 
        => Buffer.ThrowIfDisposed(isDisposed)
            .EnumerateSource(cancelToken)
            .Attempt(onError: ex =>
            {
                this.Dispose();
                return AsyncEnumerable.Empty<TSchema>();
            });

    public void Dispose()
    {
        if (isDisposed) 
            return;
        DisposeCore();
    }

    public ValueTask DisposeAsync()
    {
        if (isDisposed) 
            return ValueTask.CompletedTask;
        
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
