namespace Cmg.Dotnet.XCollections;

/// <summary>
/// A data structure that combines a max and min heap to calculate a moving median over N samples
/// with logirithmic time complexity.
/// </summary>
public class MedianHeap<TValue> where TValue : IComparable<TValue>, IEquatable<TValue>
{
    /// <summary>
    /// The number of samples currently used to calculate the median value
    /// </summary>
    public int Count => _minHeap.Count + _maxHeap.Count;
    /// <summary>
    /// The maximum number of samples to use for calculating the moving median
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    /// A circular buffer that maps each data sample to a heap node
    /// </summary>
    private readonly List<BinaryHeapNode<TValue>?> _rollingMap;
    /// <summary>
    /// An index into the circular buffer where the next sample will be placed. The index rolls
    /// back over to 0.
    /// </summary>
    private int _rollingIndex;

    // lower values stored in the max heap
    private readonly BinaryHeap<TValue> _maxHeap;
    // higher values stored in the min heap
    private readonly BinaryHeap<TValue> _minHeap;

    /// <summary>
    /// Construct an instance of a moving median heap
    /// </summary>
    /// <param name="capacity">The max number of samples to calculate the moving median for</param>
    /// <exception cref="ArgumentException"></exception>
    public MedianHeap(int capacity)
    {
        if (capacity < 1)
        {
            throw new ArgumentException("Capacity must be at least 1");
        }

        Capacity = capacity;
        _rollingMap = Enumerable.Repeat<BinaryHeapNode<TValue>?>(null, Capacity).ToList();
        _rollingIndex = 0;

        // if the sample size is odd, ensure the max heap has a size that is one greater
        var minHeapCapacity = Capacity / 2;
        var maxHeapCapacity = minHeapCapacity;
        if ((Capacity % 2) == 1)
        {
            maxHeapCapacity += 1;
        }
        _maxHeap = new BinaryHeap<TValue>(isMaxHeap: true, maxHeapCapacity);
        _minHeap = new BinaryHeap<TValue>(isMaxHeap: false, minHeapCapacity);
    }

    /// <summary>
    /// Yields the median values: If number of samples is odd, yields one median value, if number of
    /// samples is even, yields two median values.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public IEnumerable<TValue> MedianValues
    {
        get
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("No entries yet!");
            }

            // The logic has been designed to
            // 1) keep the heaps the same size if the number of samples is even
            // 2) make the max heap one element larger if the number of samples is odd.
            // when the number of samples is odd, the median value is stored in the root of the max
            // heap and when the number of samples is even, the median value is the average of the
            // values stored in the two heap roots.
            yield return _maxHeap.Root!.Value;
            if (_maxHeap.Count == _minHeap.Count)
            {
                yield return _minHeap.Root!.Value;
            }
        }
    }

    /// <summary>
    /// Adds a new sample to the moving median calculation
    /// </summary>
    /// <param name="value"></param>
    public void Add(TValue value)
    {
        if (Count == Capacity)
        {
            // we have reached the max number of samples
            // replace the value in the oldest sample
            var nodeToUpdate = _rollingMap[_rollingIndex]!;
            nodeToUpdate.Heap!.Update(nodeToUpdate, value);
        }
        else
        {
            // we havent reached the max sample size yet.
            // alternate between adding to min and max heap to keep the heaps the same size or
            // the max heap one size larger to ensure the median value(s) are always stored in
            // the roots.
            var isEven = Count % 2 == 0;
            var heapToAdd = isEven ? _maxHeap : _minHeap;
            _rollingMap[_rollingIndex] = heapToAdd.Add(value);
        }

        // at this point, the min and max heaps both satisfy their heap property indepedently,
        // but, it's possible the heap we added/updated the value in wasnt the correct heap to
        // ensure the two middle values are at the heap roots. So swap the roots then fix up the
        // heaps again, if necessary.
        var minHeapRoot = _minHeap.Root;
        var maxHeapRoot = _maxHeap.Root;
        if ((minHeapRoot != null) && (maxHeapRoot != null))
        {
            // min heap root value should be greater than max heap root value
            if (minHeapRoot.Value.CompareTo(maxHeapRoot.Value) < 0)
            {
                _minHeap.SwapRoot(_maxHeap);
            }
        }

        _rollingIndex += 1;
        _rollingIndex %= Capacity;
    }
}
