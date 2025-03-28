namespace Cmg.Dotnet.XCollections;

/// <summary>
/// A thread safe implementation of a Least Recently Used cache
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
public class LruCache<TKey, TValue> where TKey : notnull
{
    private long? _capacity = null;

    /// <summary>
    /// The maximum number of items that can be added to the cache. If null, the cache is unlimited.
    /// </summary>
    /// <exception cref="ArgumentException">If capacity is set to a value less than 1</exception>
    public long? Capacity
    {
        get
        {
            return _capacity;
        }
        set
        {
            if (value.HasValue && (value < 1))
            {
                throw new ArgumentException("Capacity must be greater than 0");
            }
            _capacity = value;
        }
    }

    private readonly SequentialDictionary<TKey, TValue> _orderedCache;
    private readonly object _lock = new();

    /// <summary>
    /// Constructs an instance of an LruCache
    /// </summary>
    /// <param name="capacity">The maximum number of items that can be added to the cache. If null, the cache is unlimited.</param>
    public LruCache(long? capacity = null)
    {
        _orderedCache = new SequentialDictionary<TKey, TValue>();

        Capacity = capacity;
    }

    /// <summary>
    /// Gets the number of items currently in the cache.
    /// </summary>
    public int Count => _orderedCache.Count;

    /// <summary>
    /// Gets the item with specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns>
    /// The item with the specified key or null if key is not found. If the key is found, the
    /// associated item is moved to the end of the cache.
    /// </returns>
    public TValue? Get(TKey key)
    {
        lock (_lock)
        {
            if (_orderedCache.TryGetValue(key, out var value))
            {
                // any time an item is retrieved, move it to the end of the cache
                _orderedCache.MoveToLast(key);
            }

            return value;
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
            // call Get so if item exists it is moved to end of cache
            var item = Get(key);

            // if item wasnt found then we will be adding a new entry
            // WARNING: What if null is a valid value?
            if (item == null)
            {
                // if there isnt room left, delete oldest entry from cache
                if (Count == Capacity)
                {
                    _orderedCache.RemoveFirst();
                }
            }

            // add or update cached value
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
