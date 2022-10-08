using AsyncFlows.Extensions;

public static partial class Extensions
{
    public static IAsyncEnumerable<T> Attempt<T>(
        this Func<IAsyncEnumerable<T>> iterator,
        Func<Exception, IAsyncEnumerable<T>> onError,
        Func<Exception, bool>? canHandle = default)
        where T : class
    {
        while (true)
        {
            var shouldHandleEx = SetIsErrorDecision(canHandle);            
            var enumerable = iterator().GetAsyncEnumerator()
                .ExposeAsyncMoveNext(onError, shouldHandleEx);
            return enumerable;
        }

    }

    private static Func<Exception, bool> SetIsErrorDecision(
        Func<Exception, bool>? isError = default) 
        => isError switch
        {
            not null => isError,
            _ => (Exception ex) => ex.IsKnownException()
        };

    private static async IAsyncEnumerable<T> ExposeAsyncMoveNext<T>(
        this IAsyncEnumerator<T> enumerator,
        Func<Exception, IAsyncEnumerable<T>> onError,
        Func<Exception, bool> isError)
        where T : class
    {
        T? message = default!;
        try
        {
            var isMore = await enumerator.MoveNextAsync();
            if (isMore)
                message = enumerator.Current;
            else
                message = null;
        }
        catch (Exception ex) when (isError(ex))
        {
            message = await ex.CatchAsyncFallback(onError);
        }
        if (message is not null)
            yield return message;
        else
            yield break;
    }

    private static async ValueTask<T> CatchAsyncFallback<T>(
        this Exception ex,
        Func<Exception, IAsyncEnumerable<T>> onError)
        where T : class
    {
        T? message = default!;
        var fallback = onError(ex).GetAsyncEnumerator();
        message = await fallback.MoveNextAsync() switch
        {
            true => fallback.Current,
            _ => default!
        };
        return message;
    }
}
