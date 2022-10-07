using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

public sealed record Envelope<TPayload>(
    TPayload Payload,
    string CurrentId,
    string CausationId)
    : Envelope(CurrentId, CausationId);

public interface IAsyncMediator
{
    ValueTask SubmitAsync<TItem>(TItem item, CancellationToken cancelToken = default);
    IAsyncEnumerable<TOut> SubmitAsync<TItem, TOut>(TItem item, CancellationToken cancelToken = default);
}
public class WIP
{
    
}
