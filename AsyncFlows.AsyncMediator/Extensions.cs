using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace AsyncFlows.AsyncMediator;

public static class Extensions
{
    public static async ValueTask<bool> SendAsync<T>(
        this ITargetBlock<T> block,
        T item,
        CancellationToken cancelToken)
    {
        var submitted = false;
        while (!submitted && !block.Completion.IsCompleted)
        {
            cancelToken.ThrowIfCancellationRequested();
            submitted = await block.SendAsync(item, cancelToken);
            await Task.Yield();
        }
        return submitted;
    }

    [return: NotNull]
    public static T ThrowIfDisposed<T>(
        this T? target,
        bool isDisposed)
        => (target, isDisposed) switch
        {
            (_, true) => throw new ObjectDisposedException(typeof(T).Name),
            (null, _) => throw new ObjectDisposedException(typeof(T).Name),
            (not null, false) => target,
        };


    internal static async IAsyncEnumerable<TItem> ToAsyncEnumerable<TItem>(
        this ISourceBlock<TItem> source,
        [EnumeratorCancellation] CancellationToken cancelToken)
    {
        while (await source.OutputAvailableAsync(cancelToken))
            yield return await source.ReceiveAsync(cancelToken);
    }

    public static bool IsCompleted(this IDataflowBlock block)
        => block.Completion.IsCompleted;

    public static bool IsNotCompleted(this IDataflowBlock block)
        => block.Completion.IsCompleted;

    public static TOut ToKnownType<TOut>(
        this object input,
        [CallerArgumentExpression("input")] string? argExpr = default)
        => input?.GetType() is TOut output
        ? output
        : throw new InvalidCastException(
            $"{argExpr} did not produce expected type. " +
            $"Expected:{typeof(TOut).Name} " +
            $"Received:{input?.GetType().Name}");
}
