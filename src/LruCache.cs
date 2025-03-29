using System.Diagnostics.CodeAnalysis;

namespace Cmg.Dotnet.XCollections;

/// <summary>
/// A thread safe implementation of a Least Recently Used cache
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class LruCache<TKey, TValue> where TKey : notnull
{
    private readonly SequentialDictionary<TKey, TValue> _orderedCache;
    private readonly object _lock = new();

    /// <summary>
    /// Constructs an instance of an LruCache
    /// </summary>
    /// <param name="capacity">The maximum number of items that can be added to the cache. If null, the cache is unlimited.</param>
    /// <exception cref="ArgumentException">If capacity is set to a value less than 1</exception>
    public LruCache(long? capacity = null)
    {
        _orderedCache = new SequentialDictionary<TKey, TValue>();

        if (capacity.HasValue && (capacity < 1))
        {
            throw new ArgumentException("Capacity must be greater than 0");
        }
        Capacity = capacity;
    }

    /// <summary>
    /// The maximum number of items that can be added to the cache. If null, the cache is unlimited.
    /// </summary>
    public long? Capacity { get; } = null;

    /// <summary>
    /// Gets the number of items currently in the cache.
    /// </summary>
    public int Count => _orderedCache.Count;

    /// <summary>
    /// Tries to get the item with specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>
    /// True if the item is found, false otherwise.
    /// </returns>
    public bool TryGet(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        lock (_lock)
        {
            if (!_orderedCache.TryGetValue(key, out value))
            {
                return false;
            }

            // any time an item is retrieved, move it to the end of the cache
            // NOTE: Inefficient. one more key lookup than necessary due to TryGetValue called above
            _orderedCache.MoveToLast(key);

            return true;
        }
    }

    /// <summary>
    /// Adds or Updates an item in the cache
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    public void Put(TKey key, TValue value)
    {
        lock (_lock)
        {
            // call TryGet so if item exists it is moved to end of cache
            var found = TryGet(key, out var item);
            if (!found)
            {
                // if item wasnt found then we will be adding a new entry. If there isnt room
                // left, delete oldest entry from cache.
                if (Count >= Capacity)
                {
                    _orderedCache.RemoveFirst();
                }
            }

            // add or update cached value
            // NOTE: Inefficient. one more key lookup than necessary due to Get called above
            _orderedCache[key] = value;
        }
    }

    /// <summary>
    /// Deletes an item from the cache.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool Delete(TKey key)
    {
        lock (_lock)
        {
            return _orderedCache.Remove(key);
        }
    }
}
