using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace AsyncFlows.AsyncMediator;

public static class Nullability
{
    [return: NotNull]
    public static T NotNull<T>([NotNull] this T? item,
        string? message = default,
        [CallerArgumentExpression("item")] string? argName = default)
        => item ?? throw new ArgumentNullException(argName, message);

    [return: NotNull]
    public static async Task<T> NotNull<T>([NotNull] this Task<T?> item,
        string? message = default,
        [CallerArgumentExpression("item")] string? argName = default)
        => await item ?? throw new ArgumentNullException(argName, message);
}
