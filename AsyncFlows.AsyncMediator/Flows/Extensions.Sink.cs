using System.Runtime.CompilerServices;
using System.Threading.Tasks.Dataflow;

namespace AsyncFlows.Internal.Extensions;

public static partial class Extensions
{
    internal static Func<IAsyncEnumerable<TSchema>> EnumerateSource<TSchema>(
        this ISourceBlock<TSchema> source,
        CancellationToken cancelToken)
        => () => Enumeration(source, cancelToken);

    internal static async IAsyncEnumerable<TSchema> Enumeration<TSchema>(
        ISourceBlock<TSchema> source,
        [EnumeratorCancellation] CancellationToken cancelToken)
    {
        while (source.IsNotCompleted() && await source.OutputAvailableAsync(cancelToken))
            yield return await source.ReceiveAsync(cancelToken);
        CloseOutEnumeration(source, cancelToken);
    }

    internal static void CloseOutEnumeration<TSchema>(
        ISourceBlock<TSchema> source,
        CancellationToken cancelToken)
    {
        if (source.IsCompleted())
            throw new InvalidOperationException($"{nameof(IsCompleted)}");
        cancelToken.ThrowIfCancellationRequested();
    }
}
