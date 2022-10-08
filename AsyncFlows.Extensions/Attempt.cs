namespace AsyncFlows.Extensions;

public static partial class Extensions
{
    public static async ValueTask<TReturn> Attempt<TReturn>(
        this Func<ValueTask<TReturn>> func,
        Func<Exception, ValueTask<TReturn>> onError,
        Func<Exception, bool>? canHandle = default)
    {
        try
        {
            return await func();
        }
        catch (Exception ex)
        when (canHandle?.Invoke(ex) ?? ex.IsKnownException())
        {
            return await onError(ex);
        }
    }
}
