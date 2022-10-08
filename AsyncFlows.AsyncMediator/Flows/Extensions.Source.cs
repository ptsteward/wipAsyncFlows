using System.Threading.Tasks.Dataflow;

namespace AsyncFlows.Internal.Extensions;

public static partial class Extensions
{
    internal static Func<ValueTask<bool>> OfferAsync<TSchema>(
        this ITargetBlock<TSchema> block,
        TSchema message,
        TimeSpan timeout,
        CancellationToken cancelToken)
        where TSchema : Envelope
        => async () =>
        {
            var submitted = false;
            while (!submitted && block.IsNotCompleted())
            {
                cancelToken.ThrowIfCancellationRequested();
                submitted = await block.SendAsync(message, cancelToken)
                    .WaitAsync(timeout, cancelToken);
                await Task.Yield();
            }
            return submitted;
        };
}
