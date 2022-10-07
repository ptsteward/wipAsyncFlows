namespace AsyncFlows.AsyncMediator;

public interface IFlowSource<TItem>
    : IAsyncDisposable, IDisposable
    where TItem : Envelope
{
    ValueTask<bool> EmitAsync(TItem item, CancellationToken cancelToken = default);
}
