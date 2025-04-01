namespace Cmg.Dotnet.XCollections;

using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

/// <summary>
/// A doubly linked list with dictionary-like access to each node via a unique key.
/// </summary>
/// <remarks>
/// This class is not thread safe.
/// </remarks>
public sealed class HashedLinkedList<TKey, TValue> :
    ICollection<KeyValuePair<TKey, TValue>>,
    IDeserializationCallback,
    ISerializable
    where TKey : notnull
{
    /// <summary>
    /// The dictionary that maps keys to nodes in the linked list.
    /// </summary>
    private readonly IDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> _dict;
    /// <summary>
    /// The underlying linked list. Composition is used instead of inheriting from the LinkedList
    /// class because the linked list and dictionary are coupled. If the LinkedList class were to be
    /// modified in the future, it could break the coupling.
    /// </summary>
    private readonly LinkedList<KeyValuePair<TKey, TValue>> _storage;

    /// <summary>
    /// Constructs a new instance of a HashedLinkedList
    /// </summary>
    public HashedLinkedList()
    {
        _dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
        _storage = new LinkedList<KeyValuePair<TKey, TValue>>();
    }

    /// <inheritdoc/>
    public bool IsReadOnly { get; }

    /// <summary>
    /// Returns the first node in the list or null if list is empty
    /// </summary>
    public LinkedListNode<KeyValuePair<TKey, TValue>>? First => _storage.First;
    /// <summary>
    /// Returns the last node in the list or null if list is empty
    /// </summary>
    public LinkedListNode<KeyValuePair<TKey, TValue>>? Last => _storage.Last;

    /// <inheritdoc/>
    public int Count => _dict.Count;

    /// <summary>
    /// Returns the node with the specified key
    /// </summary>
    /// <param name="key"></param>
    /// <exception cref="KeyNotFoundException"></exception>
    public LinkedListNode<KeyValuePair<TKey, TValue>> this[TKey key] => _dict[key];

    /// <inheritdoc/>
    public bool ContainsKey(TKey key)
    {
        return _dict.ContainsKey(key);
    }

    /// <inheritdoc/>
    public bool TryGetValue(
        TKey key,
        [MaybeNullWhen(false)] out LinkedListNode<KeyValuePair<TKey, TValue>> value
    )
    {
        return _dict.TryGetValue(key, out value);
    }

    /// <inheritdoc/>
    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return _storage.Contains(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _dict.Clear();
        _storage.Clear();
    }

    /// <summary>
    /// Add an item to the end of the list
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public void Add(TKey key, TValue value)
    {
        Add(new KeyValuePair<TKey, TValue>(key, value));
    }

    /// <summary>
    /// Add an item to the end of the list
    /// </summary>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        AddLast(item);
    }

    /// <summary>
    /// Adds a new item to the list before the specified node
    /// </summary>
    public LinkedListNode<KeyValuePair<TKey, TValue>> AddBefore(
        LinkedListNode<KeyValuePair<TKey, TValue>> node,
        KeyValuePair<TKey, TValue> item
    )
    {
        var newNode = new LinkedListNode<KeyValuePair<TKey, TValue>>(item);
        AddBefore(node, newNode);
        return newNode;
    }

    /// <summary>
    /// Adds a new node to the list before the specified node
    /// </summary>
    public void AddBefore(
        LinkedListNode<KeyValuePair<TKey, TValue>> node,
        LinkedListNode<KeyValuePair<TKey, TValue>> newNode
    )
    {
        if (ContainsKey(node.Value.Key))
        {
            throw new ArgumentException($"key {node.Value.Key} already exists");
        }

        _storage.AddBefore(node, newNode);
        _dict[node.Value.Key] = newNode;
    }

    /// <summary>
    /// Adds a new item to the list after the specified node
    /// </summary>
    public LinkedListNode<KeyValuePair<TKey, TValue>> AddAfter(
        LinkedListNode<KeyValuePair<TKey, TValue>> node,
        KeyValuePair<TKey, TValue> item
    )
    {
        var newNode = new LinkedListNode<KeyValuePair<TKey, TValue>>(item);
        AddAfter(node, newNode);
        return newNode;
    }

    /// <summary>
    /// Adds a new node to the list after the specified node
    /// </summary>
    public void AddAfter(
        LinkedListNode<KeyValuePair<TKey, TValue>> node,
        LinkedListNode<KeyValuePair<TKey, TValue>> newNode
    )
    {
        if (ContainsKey(node.Value.Key))
        {
            throw new ArgumentException($"key {node.Value.Key} already exists");
        }

        _storage.AddAfter(node, newNode);
        _dict[node.Value.Key] = newNode;
    }

    /// <summary>
    /// Adds an item to the start of the list
    /// </summary>
    public LinkedListNode<KeyValuePair<TKey, TValue>> AddFirst(TKey key, TValue value)
    {
        return AddFirst(new KeyValuePair<TKey, TValue>(key, value));
    }

    /// <summary>
    /// Adds an item to the start of the list
    /// </summary>
    public LinkedListNode<KeyValuePair<TKey, TValue>> AddFirst(KeyValuePair<TKey, TValue> item)
    {
        var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(item);
        AddFirst(node);
        return node;
    }

    /// <summary>
    /// Adds a node to the start of the list
    /// </summary>
    public void AddFirst(LinkedListNode<KeyValuePair<TKey, TValue>> node)
    {
        if (ContainsKey(node.Value.Key))
        {
            throw new ArgumentException($"key {node.Value.Key} already exists");
        }

        _storage.AddFirst(node);
        _dict[node.Value.Key] = node;
    }

    /// <summary>
    /// Adds an item to the end of the list
    /// </summary>
    public LinkedListNode<KeyValuePair<TKey, TValue>> AddLast(TKey key, TValue value)
    {
        return AddLast(new KeyValuePair<TKey, TValue>(key, value));
    }

    /// <summary>
    /// Adds an item to the end of the list
    /// </summary>
    public LinkedListNode<KeyValuePair<TKey, TValue>> AddLast(KeyValuePair<TKey, TValue> item)
    {
        var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(item);
        AddLast(node);
        return node;
    }

    /// <summary>
    /// Adds a node to the end of the list
    /// </summary>
    public void AddLast(LinkedListNode<KeyValuePair<TKey, TValue>> node)
    {
        if (ContainsKey(node.Value.Key))
        {
            throw new ArgumentException($"key {node.Value.Key} already exists");
        }

        _storage.AddLast(node);
        _dict[node.Value.Key] = node;
    }

    /// <inheritdoc/>
    public bool Remove(TKey key)
    {
        return ContainsKey(key) && Remove(_dict[key]);
    }

    /// <inheritdoc/>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        // need to check if the item with the specified key and value exists first
        return Contains(item) && Remove(item.Key);
    }

    /// <summary>
    /// Remove the specified node from the list
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public bool Remove(LinkedListNode<KeyValuePair<TKey, TValue>> node)
    {
        var removed = _dict.Remove(node.Value.Key);
        if (!removed)
        {
            return false;
        }

        _storage.Remove(node);

        return true;
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
        return (_storage.First != null) && Remove(_storage.First);
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
        return (_storage.Last != null) && Remove(_storage.Last);
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

    /// <inheritdoc/>
    public void OnDeserialization(object? sender)
    {
        _storage.OnDeserialization(sender);
    }

    /// <inheritdoc/>
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        _storage.GetObjectData(info, context);
    }
}
