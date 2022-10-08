using System.Threading.Tasks.Dataflow;

namespace AsyncFlows.Internal.Extensions;

public static partial class Extensions
{
    internal static bool IsCompleted(this IDataflowBlock block)
        => block?.Completion.IsCompleted ?? true;

    internal static bool IsNotCompleted(this IDataflowBlock block)
        => !block?.IsCompleted() ?? false;
}
