namespace Cmg.Dotnet.XCollections;

/// <summary>
/// A data structure that combines a max and min heap to calculate a moving median over N samples
/// with logirithmic time complexity.
/// </summary>
public class MedianHeap
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
    private readonly List<BinaryHeapNode<int>?> _rollingMap;
    /// <summary>
    /// An index into the circular buffer where the next sample will be placed. The index rolls
    /// back over to 0.
    /// </summary>
    private int _rollingIndex;

    // lower values stored in the max heap
    private readonly BinaryHeap<int> _maxHeap;
    // higher values stored in the min heap
    private readonly BinaryHeap<int> _minHeap;

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
        _rollingMap = Enumerable.Repeat<BinaryHeapNode<int>?>(null, Capacity).ToList();
        _rollingIndex = 0;

        // if the sample size is odd, ensure the min heap has a size that is one greater
        var maxHeapCapacity = Capacity / 2;
        var minHeapCapacity = maxHeapCapacity;
        if ((Capacity % 2) == 1)
        {
            minHeapCapacity += 1;
        }
        _maxHeap = new BinaryHeap<int>(isMaxHeap: true, maxHeapCapacity);
        _minHeap = new BinaryHeap<int>(isMaxHeap: false, minHeapCapacity);
    }

    /// <summary>
    /// Returns twice the median value
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public int GetTwiceMedian
    {
        get
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("No entries yet!");
            }

            // The logic has been designed to
            // 1) keep the heaps the same size if the number of samples is even
            // 2) make the min heap one element larger if the number of samples is odd.
            // when the number of samples is odd, the median value is stored in the root of the min
            // heap and when the number of samples is even, the median value is the average of the
            // values stored in the two heap roots.
            var isEven = (Count % 2) == 0;
            var result = _minHeap.Root!.Value;
            result += isEven
                ? _maxHeap.Root!.Value
                : _minHeap.Root!.Value;

            return result;
        }
    }

    /// <summary>
    /// Adds a new sample to the moving median calculation
    /// </summary>
    /// <param name="value"></param>
    public void Add(int value)
    {
        if (Count == Capacity)
        {
            // we have reached the max number of samples
            // update the node's value
            var nodeToUpdate = _rollingMap[_rollingIndex]!;
            nodeToUpdate.Heap!.Update(nodeToUpdate, value);
        }
        else
        {
            // we havent reached the max sample size yet.
            // alternate between adding to min and max heap to ensure the heaps are the same
            // size or the min heap is one size larger.
            var isEven = Count % 2 == 0;
            var heapToAdd = isEven ? _minHeap : _maxHeap;
            _rollingMap[_rollingIndex] = heapToAdd.Add(value);
        }

        // at this point, the min and max heaps both satisfy their heap property indepedently,
        // but, it's possible we still need to swap roots and update them again to ensure the
        // root nodes contain the two middle values.
        var minHeapRoot = _minHeap.Root;
        var maxHeapRoot = _maxHeap.Root;
        if ((minHeapRoot != null) && (maxHeapRoot != null))
        {
            // min heap root value should be greater than max heap root value
            if (minHeapRoot.Value.CompareTo(maxHeapRoot.Value) < 0)
            {
                minHeapRoot.Heap = null;
                maxHeapRoot.Heap = null;
                _minHeap.ReplaceRoot(maxHeapRoot);
                _maxHeap.ReplaceRoot(minHeapRoot);
            }
        }

        _rollingIndex += 1;
        _rollingIndex %= Capacity;
    }
}
