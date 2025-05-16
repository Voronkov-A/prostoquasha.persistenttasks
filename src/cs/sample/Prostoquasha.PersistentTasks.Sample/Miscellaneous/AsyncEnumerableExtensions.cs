using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Prostoquasha.PersistentTasks.Sample.Miscellaneous;

internal static class AsyncEnumerableExtensions
{
    public static async IAsyncEnumerable<(T? First, T? Second)> FullJoinOrdered<T, TKey>(
        this IAsyncEnumerable<T> first,
        IAsyncEnumerable<T> second,
        Func<T, TKey> keySelector,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var item in FullJoinOrdered(
            first,
            second,
            keySelector,
            Comparer<TKey>.Default,
            cancellationToken))
        {
            yield return item;
        }
    }

    public static async IAsyncEnumerable<(T? First, T? Second)> FullJoinOrdered<T, TKey>(
        this IAsyncEnumerable<T> first,
        IAsyncEnumerable<T> second,
        Func<T, TKey> keySelector,
        IComparer<TKey> keyComparer,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using var firstIt = first.GetAsyncEnumerator(cancellationToken);
        await using var secondIt = second.GetAsyncEnumerator(cancellationToken);

        if (!await firstIt.MoveNextAsync())
        {
            while (await secondIt.MoveNextAsync())
            {
                yield return (default, secondIt.Current);
            }

            yield break;
        }

        if (!await secondIt.MoveNextAsync())
        {
            yield return (firstIt.Current, default);

            while (await firstIt.MoveNextAsync())
            {
                yield return (firstIt.Current, default);
            }

            yield break;
        }

        while (true)
        {
            var firstKey = keySelector(firstIt.Current);
            var secondKey = keySelector(secondIt.Current);
            var comparison = keyComparer.Compare(firstKey, secondKey);

            if (comparison < 0)
            {
                yield return (firstIt.Current, default);

                if (!await firstIt.MoveNextAsync())
                {
                    yield return (default, secondIt.Current);
                    break;
                }
            }
            else if (comparison == 0)
            {
                yield return (firstIt.Current, secondIt.Current);

                if (!await firstIt.MoveNextAsync())
                {
                    break;
                }

                if (!await secondIt.MoveNextAsync())
                {
                    yield return (firstIt.Current, default);
                    break;
                }
            }
            else
            {
                yield return (default, secondIt.Current);

                if (!await secondIt.MoveNextAsync())
                {
                    yield return (firstIt.Current, default);
                    break;
                }
            }
        }

        while (await firstIt.MoveNextAsync())
        {
            yield return (firstIt.Current, default);
        }

        while (await secondIt.MoveNextAsync())
        {
            yield return (default, secondIt.Current);
        }
    }
}
