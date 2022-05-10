using System;
using System.Collections.Concurrent;
using System.Threading;

namespace MccSoft.IntegreSqlClient.DatabaseInitialization;

public class LazyConcurrentDictionary<TKey, TValue>
{
    private readonly ConcurrentDictionary<TKey, Lazy<TValue>> _concurrentDictionary = new();

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        var lazyResult = _concurrentDictionary.GetOrAdd(
            key,
            k =>
                new Lazy<TValue>(
                    () => valueFactory(k),
                    LazyThreadSafetyMode.ExecutionAndPublication
                )
        );

        return lazyResult.Value;
    }
}
