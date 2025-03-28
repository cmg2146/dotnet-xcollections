# Additional Collection Types for .NET

## Sequential Dictionary

The Sequential Dictionary orders keys/values by least to most recently inserted. It is implemented as a
doubly linked list where access to each node in the list can be performed via the key. It has
O(1) access to any element by key and O(1) access to the first and last element. Additional methods
are provided to move any element (accessed by key) to the start or end of the list.

```csharp
var dict = new SequentialDictionary<string, int>()
{
    new("key1", 1),
    new("key2", 2)
};

dict.Add("key3", 3);
dict["key4"] = 4;
dict["key4"] = 5;
dict.MoveToLast("key1");
dict.MoveToFirst("key4");
dict.Remove("key1");
```

## LRU Cache

A Least Recently Used cache offerring Get, Put, and Delete methods. When the cache reaches the
capacity, the least recently used item will be removed.

```csharp

class MyCacheItem
{
    public int Id { get; set; }
    public string Name { get; set; }
}

var cache = new LruCache<int, MyCacheItem>(capacity=1000);

cache.Put(1, new MyCacheItem { Id = 1, Name = "user1" });
cache.Put(2, new MyCacheItem { Id = 2, Name = "user2" });
var user1 = cache.Get(1);
cache.Delete(2);
```
