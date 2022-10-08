﻿using System.Threading.Tasks.Dataflow;

namespace AsyncFlows.AsyncMediator.Flows;

internal sealed class FlowSource<TSchema>
    : IFlowSource<TSchema>,
    ILinkableSource<TSchema>
    where TSchema : Envelope
{
    private volatile bool isDisposed;
    private BroadcastBlock<TSchema>? Source { get; set; }

    public FlowSource()
        => Source = new(msg => msg,
            new()
            {
                EnsureOrdered = true,
                BoundedCapacity = DataflowBlockOptions.Unbounded
            });

    public ValueTask<bool> EmitAsync(TSchema message, CancellationToken cancelToken = default)
        => Source.ThrowIfDisposed(isDisposed)
            .OfferAsync(message, TimeSpan.FromMilliseconds(300), cancelToken)
            .Attempt(onError: ex => ValueTask.FromResult(false));

    

    IDisposable ILinkableSource<TSchema>.LinkTo(ITargetBlock<TSchema> sink)
        => Source.ThrowIfDisposed(isDisposed)
            .LinkTo(sink, new()
            {
                PropagateCompletion = true,
            });

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
