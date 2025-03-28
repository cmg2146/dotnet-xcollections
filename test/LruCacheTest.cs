namespace Cmg.Dotnet.XCollections.Test;

using Xunit;

public class LruCacheTest
{
    [Fact]
    public void TestBasicCacheControl()
    {
        _ = int.TryParse("5", out int va);

        var cache = new LruCache<string, int>();

        // put a new item in the cache
        cache.Put("1", 1);
        var found = cache.TryGet("1", out var val);
        Assert.True(found);
        Assert.True(val == 1);

        // update an item in the cache
        cache.Put("1", 2);
        found = cache.TryGet("1", out val);
        Assert.True(found);
        Assert.True(val == 2);
        Assert.True(cache.Count == 1);

        // delete an item in the cache
        cache.Delete("1");
        found = cache.TryGet("1", out val);
        Assert.False(found);
        Assert.True(cache.Count == 0);
    }

    [Fact]
    public void TestCacheCapacity()
    {
        var capacity = 3;
        var items = Enumerable
            .Range(0, capacity + 1)
            .Select(x => new KeyValuePair<string, int>(x.ToString(), x))
            .ToList();

        var cache = new LruCache<string, int>(capacity);

        foreach (var item in items)
        {
            cache.Put(item.Key, item.Value);
        }

        // verify the first item was pushed out of the cache
        var found0 = cache.TryGet(items[0].Key, out var val0);
        var found1 = cache.TryGet(items[1].Key, out var val1);
        Assert.True(cache.Count == capacity);
        Assert.True(cache.Capacity == capacity);
        Assert.False(found0);
        Assert.True(found1);
        Assert.True(val1 == items[1].Value);

        // add one more item and verify the third item (index = 2) is pushed out of the cache. Calling the Get
        // method above pushed item with index = 1 to end of cache.
        cache.Put("1000", 1);
        var found2 = cache.TryGet(items[2].Key, out var val2);
        var found3 = cache.TryGet(items[3].Key, out var val3);
        Assert.True(cache.Count == capacity);
        Assert.True(cache.Capacity == capacity);
        Assert.False(found2);
        Assert.True(found3);
        Assert.True(val3 == items[3].Value);
    }
}
