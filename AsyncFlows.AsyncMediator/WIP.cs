using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace AsyncFlows.AsyncMediator;
public interface IAsyncMediator
{
    ValueTask SubmitAsync<TSchema>(TSchema message, CancellationToken cancelToken = default);
    IAsyncEnumerable<TOut> SubmitAsync<TSchema, TOut>(TSchema message, CancellationToken cancelToken = default);
    IAsyncEnumerable<TSchema> ReceiveAsync<TSchema>(CancellationToken cancelToken = default);
}

internal sealed class AsyncMediator
    : BackgroundService,
    IAsyncMediator
{
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => throw new NotImplementedException();

    public IAsyncEnumerable<TSchema> ReceiveAsync<TSchema>(CancellationToken cancelToken = default) => throw new NotImplementedException();
    public ValueTask SubmitAsync<TSchema>(TSchema message, CancellationToken cancelToken = default) => throw new NotImplementedException();
    public IAsyncEnumerable<TOut> SubmitAsync<TSchema, TOut>(TSchema message, CancellationToken cancelToken = default) => throw new NotImplementedException();
}
