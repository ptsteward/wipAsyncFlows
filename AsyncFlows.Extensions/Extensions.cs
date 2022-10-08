using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Dataflow;

namespace AsyncFlows.Extensions;

public static partial class Extensions
{
    public static bool IsKnownException(this Exception ex)
        => ex is TimeoutException ||
            ex is OperationCanceledException ||
            ex is InvalidOperationException;

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
