using System.Diagnostics.CodeAnalysis;

namespace Cmg.Dotnet.XCollections;

/// <summary>
/// A thread safe implementation of a Least Recently Used cache
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class LruCache<TKey, TValue> where TKey : notnull
{
    private readonly HashedLinkedList<TKey, TValue> _orderedCache;
    private readonly object _lock = new();

    /// <summary>
    /// Constructs an instance of an LruCache
    /// </summary>
    /// <param name="capacity">The maximum number of items that can be added to the cache. If null, the cache is unlimited.</param>
    /// <exception cref="ArgumentException">If capacity is set to a value less than 1</exception>
    public LruCache(long? capacity = null)
    {
        _orderedCache = new HashedLinkedList<TKey, TValue>();

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
            if (!_orderedCache.TryGetValue(key, out var node))
            {
                value = default;
                return false;
            }

            Touch(node);
            value = node.Value.Value;
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
            var item = new KeyValuePair<TKey, TValue>(key, value);

            if (_orderedCache.TryGetValue(key, out var node))
            {
                // key already exists, update it
                node.Value = item;
                Touch(node);
            }
            else
            {
                // If there isnt room left to add a new entry, delete oldest entry from cache.
                if (Count >= Capacity)
                {
                    _orderedCache.RemoveFirst();
                }

                _orderedCache.AddLast(item);
            }
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

    private void Touch(LinkedListNode<KeyValuePair<TKey, TValue>> node)
    {
        // any time an item is retrieved or updated, move it to the end of the cache
        if (!ReferenceEquals(node, _orderedCache.Last))
        {
            _orderedCache.Remove(node);
            _orderedCache.AddLast(node);
        }
    }
}
