namespace AsyncFlows.AsyncMediator;

public interface IFlowSource<TSchema>
    : IAsyncDisposable, IDisposable
    where TSchema : Envelope
{
    ValueTask<bool> EmitAsync(TSchema item, CancellationToken cancelToken = default);
}
