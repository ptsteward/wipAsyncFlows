namespace AsyncFlows.AsyncMediator;

public interface IFlowSink<TItem>
    : IAsyncDisposable, IDisposable
    where TItem : Envelope
{
    IAsyncEnumerable<TItem> ConsumeAsync(CancellationToken cancelToken = default);
}
