# Additional Collection Types for .NET

## Hashed Linked List

A linked list where each node is accessible via a unique key lookup.

```csharp
var dict = new HashedLinkedList<string, int>()
{
    new("key1", 1),
    new("key2", 2)
};

dict.Add("key3", 3);
dict.AddFirst("key4", 4);
var node = dict["key1"];
dict.Remove("key1");
```

## LRU Cache

A Least Recently Used cache offerring TryGet, Put, and Delete methods. When the cache reaches the
capacity, the least recently used item will be removed.

```csharp

class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}

var cache = new LruCache<int, User>(capacity=1000);

cache.Put(1, new User { Id = 1, Name = "user1" });
cache.Put(2, new User { Id = 2, Name = "user2" });
bool found = cache.TryGet(1, out User user1);
cache.Delete(2);
```

## Median Heap

A combination of balanced, equally sized min and max heaps used to efficiently calculate a moving median.

```csharp
var maxSamples = 3;
var medianHeap = new MedianHeap<int>(maxSamples);

medianHeap.Add(6); // median now 6
medianHeap.Add(2); // median now (6 + 2) / 2
medianHeap.Add(4); // median now 4
medianHeap.Add(1); // replaces sample with value 6, median now 2.

 // returns the median values (1 value if current # of samples is odd, two values if even)
medianHeap.MedianValues.ToList();
```
