namespace Cmg.Dotnet.XCollections;

/// <summary>
/// A node in a BinaryHeap
/// </summary>
/// <typeparam name="TValue"></typeparam>
internal class BinaryHeapNode<TValue> where TValue : IComparable<TValue>, IEquatable<TValue>
{
    /// <summary>
    /// The value stored in the node
    /// </summary>
    public TValue Value { get; internal set; } = default!;
    /// <summary>
    /// The index of this node in the underlying heap array. Used internally for bidirectional linking.
    /// </summary>
    public int Index { get; internal set; }
    /// <summary>
    /// The heap this node belongs to
    /// </summary>
    public BinaryHeap<TValue> Heap { get; internal set; } = null!;
}
