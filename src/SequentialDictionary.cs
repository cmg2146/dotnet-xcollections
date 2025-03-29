namespace Cmg.Dotnet.XCollections;

using System.Diagnostics.CodeAnalysis;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A dictionary that uses a doubly linked list for underlying storage such that the most recently
/// added item occupies the last element in the linked list and the oldest item occupies the first
/// element in the list.
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
/// <remarks>
/// This class is not thread safe.
/// </remarks>
public class SequentialDictionary<TKey, TValue> : IDictionary<TKey, TValue> where TKey : notnull
{
    private readonly IDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _dict;
    private readonly LinkedList<KeyValuePair<TKey, TValue>> _storage;

    /// <summary>
    /// Constructs an instance of a SequentialDictionary
    /// </summary>
    public SequentialDictionary()
    {
        _dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
        _storage = new LinkedList<KeyValuePair<TKey, TValue>>();
    }

    /// <inheritdoc/>
    public bool IsReadOnly { get; }
    /// <inheritdoc/>
    public int Count => _dict.Count;

    /// <inheritdoc/>
    public TValue this[TKey key]
    {
        get
        {
            // dont catch exceptions and let them bubble up if key is not found
            return _dict[key].Value.Value;
        }
        set
        {
            var item = new KeyValuePair<TKey, TValue>(key, value);

            if (ContainsKey(key))
            {
                _dict[key].Value = item;
            }
            else
            {
                Add(item);
            }
        }
    }

    /// <inheritdoc/>
    // iterate over the storage instead of the dictionary to return keys in correct order
    public ICollection<TKey> Keys => _storage.Select(x => x.Key).ToList();
    /// <inheritdoc/>
    public ICollection<TValue> Values => _storage.Select(x => x.Value).ToList();

    /// <inheritdoc/>
    public void Clear()
    {
        _dict.Clear();
        _storage.Clear();
    }

    /// <inheritdoc/>
    public void Add(TKey key, TValue value)
    {
        Add(new KeyValuePair<TKey, TValue>(key, value));
    }

    /// <inheritdoc/>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        if (ContainsKey(item.Key))
        {
            throw new ArgumentException($"key {item.Key} already exists");
        }

        _dict[item.Key] = _storage.AddLast(item);
    }

    /// <inheritdoc/>
    public bool Remove(TKey key)
    {
        return ContainsKey(key) && RemoveNode(_dict[key]);
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        // need to check if the item with the specified key and value exists first
        return Contains(item) && Remove(item.Key);
    }

    /// <summary>
    /// Removes the first element from the dictionary.
    /// </summary>
    ///
    /// <remarks>
    /// The first element in the dictionary is always stored as the first element in the underlying
    /// linked list.
    /// </remarks>
    public bool RemoveFirst()
    {
        return (_storage.First != null) && RemoveNode(_storage.First);
    }

    /// <summary>
    /// Removes the last element from the dictionary.
    /// </summary>
    ///
    /// <remarks>
    /// The last element in the dictionary is always stored as the last element in the underlying
    /// linked list.
    /// </remarks>
    public bool RemoveLast()
    {
        return (_storage.Last != null) && RemoveNode(_storage.Last);
    }

    private bool RemoveNode(LinkedListNode<KeyValuePair<TKey, TValue>> node)
    {
        var removed = _dict.Remove(node.Value.Key);
        if (!removed)
        {
            return false;
        }

        _storage.Remove(node);

        return true;
    }

    /// <inheritdoc/>
    public bool ContainsKey(TKey key)
    {
        return _dict.ContainsKey(key);
    }

    /// <inheritdoc/>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (!_dict.TryGetValue(key, out var node))
        {
            value = default;
            return false;
        }

        value = node.Value.Value;
        return true;
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _storage.Contains(item);
    }

    /// <inheritdoc/>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        _storage.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _storage.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Move the specified entry to the start of the dictionary.
    /// </summary>
    /// <exception cref="KeyNotFoundException">key is not found</exception>
    public void MoveToFirst(TKey key)
    {
        // will raise an exception if key is not found
        var node = _dict[key];
        if (!ReferenceEquals(node, _storage.First))
        {
            _storage.Remove(node);
            _storage.AddFirst(node);
        }
    }

    /// <summary>
    /// Move the specified entry to the end of the dictionary.
    /// </summary>
    /// <exception cref="KeyNotFoundException">key is not found</exception>
    public void MoveToLast(TKey key)
    {
        // will raise an exception if key is not found
        var node = _dict[key];
        if (!ReferenceEquals(node, _storage.Last))
        {
            _storage.Remove(node);
            _storage.AddLast(node);
        }
    }

    /// <summary>
    /// Returns the last item in the dictionary.
    /// </summary>
    /// <exception cref="InvalidOperationException">Dictionary is empty</exception>
    /// <remarks>
    /// The <seealso cref="Enumerable.Last"/> extension method has been overloaded here
    /// for increased performance. That method requires iterating from the start of the list.
    /// </remarks>
    public KeyValuePair<TKey, TValue> Last()
    {
        return _storage.Last == null
            ? throw new InvalidOperationException("Dictionary is empty")
            : _storage.Last.Value;
    }

    /// <summary>
    /// Returns the last item in the dictionary, or null if empty.
    /// </summary>
    /// /// <remarks>
    /// The <seealso cref="Enumerable.LastOrDefault"/> extension method has been overloaded here
    /// for increased performance. That method requires iterating from the start of the list.
    /// </remarks>
    public KeyValuePair<TKey, TValue>? LastOrDefault()
    {
        return _storage.Last?.Value;
    }
}
