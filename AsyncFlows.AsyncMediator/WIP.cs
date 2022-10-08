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
    ValueTask SubmitAsync<TItem>(TItem item, CancellationToken cancelToken = default);
    IAsyncEnumerable<TOut> SubmitAsync<TItem, TOut>(TItem item, CancellationToken cancelToken = default);
}

internal sealed class AsyncMediator
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        => throw new NotImplementedException();
}
