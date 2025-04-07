using System.ComponentModel;
using System.Globalization;

using Microsoft.VisualBasic;

namespace Cmg.Dotnet.XCollections;

/// <summary>
/// A node in a BinaryHeap
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class BinaryHeapNode<TValue> where TValue : IComparable<TValue>, IEquatable<TValue>
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
    public BinaryHeap<TValue>? Heap { get; internal set; } = null;
}

/// <summary>
/// A binary heap data structure supporting min or max heaps.
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class BinaryHeap<TValue> where TValue : IComparable<TValue>, IEquatable<TValue>
{
    /// <summary>
    /// True if max heap, false if min heap
    /// </summary>
    public bool IsMaxHeap { get; init; }
    /// <summary>
    /// Return the current number of items in the heap
    /// </summary>
    public int Count { get; private set; }
    /// <summary>
    /// The maximum number of items allowed in the heap
    /// </summary>
    public int Capacity { get; init; }
    /// <summary>
    /// Returns the root node in the heap
    /// </summary>
    public BinaryHeapNode<TValue>? Root => _heap.FirstOrDefault();

    /// <summary>
    /// Underlying storage for the heap. Index 0 is the tree root and numbering continues down to the
    /// next level, incrementing from left to right until the current level is full. Given an index to
    /// a node, the node's left child would be at (index * 2) + 1 and right child at (index * 2) + 2.
    /// The calculations can be reversed to find the parent index for a given node.
    /// </summary>
    private readonly List<BinaryHeapNode<TValue>?> _heap;

    /// <summary>
    /// Construct a binary heap instance
    /// </summary>
    /// <param name="isMaxHeap">Indicates if heap is a min or max heap</param>
    /// <param name="capacity">The maximum number of items to place in the heap</param>
    /// <exception cref="ArgumentException"></exception>
    public BinaryHeap(bool isMaxHeap, int capacity)
    {
        if (capacity < 1)
        {
            throw new ArgumentException("Capacity must be at least 1");
        }

        IsMaxHeap = isMaxHeap;
        Capacity = capacity;
        Count = 0;
        _heap = Enumerable.Repeat<BinaryHeapNode<TValue>?>(null, Capacity).ToList();
    }

    /// <summary>
    /// Adds a new item to the heap
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public BinaryHeapNode<TValue> Add(TValue value)
    {
        if (Count == Capacity)
        {
            throw new InvalidOperationException("Heap is full!");
        }

        // add the node to the first free element in the heap array
        var node = new BinaryHeapNode<TValue>
        {
            Value = value,
            Index = Count,
            Heap = this
        };
        _heap[node.Index] = node;
        Count++;

        // now fix up the heap to maintain the heap property.
        Update(node, true);

        return node;
    }

    /// <summary>
    /// Remove the specified node from the heap
    /// </summary>
    /// <param name="node"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Remove(BinaryHeapNode<TValue> node)
    {
        throw new NotImplementedException("Not implemented yet");
    }

    /// <summary>
    /// Updates the value in a node and fixes up the heap to maintain the heap property
    /// </summary>
    /// <param name="node"></param>
    /// <param name="value"></param>
    public void Update(BinaryHeapNode<TValue> node, TValue value)
    {
        var oldValue = node.Value;

        // if the value is the same, do nothing
        if (!value.Equals(oldValue))
        {
            node.Value = value;
            Update(node, ShouldMoveUp(value, oldValue));
        }
    }

    /// <summary>
    /// Swaps the root of this heap with the root of the other heap, fixing up both heaps to
    /// maintain the heap property.
    /// </summary>
    /// <param name="otherHeap"></param>
    public void SwapRoot(BinaryHeap<TValue> otherHeap)
    {
        if ((Root == null) || (otherHeap.Root == null))
        {
            throw new InvalidOperationException("One of the heaps is empty! Can't swap roots");
        }

        var (ourRoot, theirRoot) = (otherHeap.Root, Root);
        ourRoot.Heap = this;
        theirRoot.Heap = otherHeap;
        _heap[0] = ourRoot;
        otherHeap._heap[0] = theirRoot;

        // make sure to fix up the heaps
        Update(ourRoot, false);
        otherHeap.Update(theirRoot, false);
    }

    /// <summary>
    /// Updates the heap to maintain the heap property when a node's value changes.
    /// </summary>
    /// <param name="node">The node that was updated</param>
    /// <param name="moveUp">Indicates which direction to move the node (up or down the tree)</param>
    private void Update(BinaryHeapNode<TValue> node, bool moveUp)
    {
        var continueSwapping = true;
        while (continueSwapping)
        {
            var swapWith = moveUp
                ? GetSwapUp(node)
                : GetSwapDown(node);

            if (swapWith != null)
            {
                SwapNodes(node, swapWith);
            }
            else
            {
                continueSwapping = false;
            }
        }
    }

    /// <summary>
    /// Returns the parent node of the specified node if the node needs to be moved up to satisfy
    /// the heap property. Returns null if the node does not need to be moved up.
    /// </summary>
    /// <param name="node"></param>
    private BinaryHeapNode<TValue>? GetSwapUp(BinaryHeapNode<TValue> node)
    {
        BinaryHeapNode<TValue>? result = null;

        var parent = GetParent(node);
        if ((parent != null) && ShouldMoveUp(node.Value, parent.Value))
        {
            result = parent;
        }

        return result;
    }

    /// <summary>
    /// Returns a child node of the specified node if the node needs to be moved down to satisfy
    /// the heap property. Returns null if the node does not need to be moved down.
    /// </summary>
    /// <param name="node"></param>
    private BinaryHeapNode<TValue>? GetSwapDown(BinaryHeapNode<TValue> node)
    {
        BinaryHeapNode<TValue>? result = null;

        var candidateChild = GetMaxOrMinChild(node);
        if ((candidateChild != null) && ShouldMoveDown(node.Value, candidateChild.Value))
        {
            result = candidateChild;
        }

        return result;
    }

    /// <summary>
    /// Indicates if the first value should be moved up in the heap, relative to the second value, to
    /// satisfy the heap property.
    /// </summary>
    /// <param name="firstValue"></param>
    /// <param name="secondValue"></param>
    /// <returns></returns>
    private bool ShouldMoveUp(TValue firstValue, TValue secondValue)
    {
        var valComparison = firstValue.CompareTo(secondValue);
        return IsMaxHeap
            ? (valComparison > 0)
            : (valComparison < 0);
    }

    /// <summary>
    /// Indicates if the first value should be moved down in the heap, relative to the second value, to
    /// satisfy the heap property.
    /// </summary>
    /// <param name="firstValue"></param>
    /// <param name="secondValue"></param>
    /// <returns></returns>
    private bool ShouldMoveDown(TValue firstValue, TValue secondValue)
    {
        var valComparison = firstValue.CompareTo(secondValue);
        return IsMaxHeap
            ? (valComparison < 0)
            : (valComparison > 0);
    }

    /// <summary>
    /// Swaps two connected nodes in the heap
    /// </summary>
    /// <param name="node"></param>
    /// <param name="otherNode"></param>
    private void SwapNodes(BinaryHeapNode<TValue> node, BinaryHeapNode<TValue> otherNode)
    {
        var oldIndex = node.Index;
        var oldOtherIndex = otherNode.Index;

        _heap[oldIndex] = otherNode;
        otherNode.Index = oldIndex;
        _heap[oldOtherIndex] = node;
        node.Index = oldOtherIndex;
    }

    /// <summary>
    /// Returns the parent of a given node, or null if the node is the root node
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private BinaryHeapNode<TValue>? GetParent(BinaryHeapNode<TValue> node)
    {
        if (node.Index < 1)
        {
            return null;
        }

        // even indexed nodes are always righthand children.
        var offset = ((node.Index % 2) == 0) ? 1 : 0;
        var parentIndex = (node.Index - offset - 1) / 2;

        return _heap[parentIndex];
    }

    /// <summary>
    /// Returns the left child of a given node, or null if the node doesnt have a left child
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private BinaryHeapNode<TValue>? GetLeftChild(BinaryHeapNode<TValue> node)
    {
        BinaryHeapNode<TValue>? result = null;

        // odd indexed nodes are always lefthand children.
        var childIndex = (node.Index * 2) + 1;
        if (childIndex < Capacity)
        {
            result = _heap[childIndex];
        }

        return result;
    }

    /// <summary>
    /// Returns the right child of a given node, or null if the node doesnt have a right child
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private BinaryHeapNode<TValue>? GetRightChild(BinaryHeapNode<TValue> node)
    {
        BinaryHeapNode<TValue>? result = null;

        // even indexed nodes are always righthand children.
        var childIndex = (node.Index * 2) + 2;
        if (childIndex < Capacity)
        {
            result = _heap[childIndex];
        }

        return result;
    }

    /// <summary>
    /// Gets the child with the greater value (if max heap) or smaller value (min heap). Returns
    /// null if the node does not have any children.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private BinaryHeapNode<TValue>? GetMaxOrMinChild(BinaryHeapNode<TValue> node)
    {
        BinaryHeapNode<TValue>? result;

        var leftChild = GetLeftChild(node);
        var rightChild = GetRightChild(node);

        if (leftChild == null)
        {
            // right child may still be null, but thats ok, returning null indicates no children
            result = rightChild;
        }
        else if (rightChild == null)
        {
            // left child may still be null, but thats ok, returning null indicates no children
            result = leftChild;
        }
        else
        {
            // use the largest child for max heap and smallest for min heap
            result = ShouldMoveUp(leftChild.Value, rightChild.Value)
                ? leftChild
                : rightChild;
        }

        return result;
    }
}
