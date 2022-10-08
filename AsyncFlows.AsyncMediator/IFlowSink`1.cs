namespace AsyncFlows.AsyncMediator;

public interface IFlowSink<TSchema>
    : IAsyncDisposable, IDisposable
    where TSchema : Envelope
{
    IAsyncEnumerable<TSchema> ConsumeAsync(CancellationToken cancelToken = default);
}
