namespace Cmg.Dotnet.XCollections.Test;

using Xunit;

using Cmg.Dotnet.XCollections;

public class HashedLinkedListTest
{

    [Fact]
    public void TestTryGetValue()
    {
        var invalidKey = "keynotfound";
        var entry = new KeyValuePair<string, int>("key", 1);
        var dict = new HashedLinkedList<string, int>();

        var node = dict.AddLast(entry);

        // lookup a key that exists
        var found = dict.TryGetValue(entry.Key, out var val);
        Assert.True(found);
        Assert.True(ReferenceEquals(val, node));

        // lookup a key that doesnt exist.
        found = dict.TryGetValue(invalidKey, out var val2);
        Assert.False(found);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    public void TestContainsKey(int key)
    {
        var otherKey = key+4;
        var invalidKey = key+2;
        var dict = new HashedLinkedList<int, int>();
        dict.AddLast(key, 1);
        dict.AddLast(otherKey, 1);

        Assert.True(dict.ContainsKey(key), $"Dictionary should contain key '{key}'");
        Assert.True(dict.ContainsKey(otherKey), $"Dictionary should contain key '{otherKey}'");
        Assert.False(dict.ContainsKey(invalidKey), $"Dictionary should not contain key '{invalidKey}'");
    }

    [Fact]
    public void TestAddAndContains()
    {
        var validValues = new List<KeyValuePair<string, int>>
        {
            new("1", 1),
            new("2", 2)
        };
        var invalidValues = new List<KeyValuePair<string, int>>
        {
            new("1", 2),
            new("2", 1)
        };

        // create the dictionary and add the valid values to it
        var dict = new HashedLinkedList<string, int>();
        validValues.ForEach(entry => dict.Add(entry));

        foreach(var validValue in validValues)
        {
            Assert.True(dict.Contains(validValue), $"Dictionary should contain '{validValue}'");
        }

        foreach(var invalidValue in invalidValues)
        {
            Assert.False(dict.Contains(invalidValue), $"Dictionary should not contain '{invalidValue}'");
        }

        // verify correct amount of items in the dictionary.
        Assert.True(validValues.Count == dict.Count, $"Dictionary Count should be {validValues.Count}");

        // try adding a key that already exists and verify exception is thrown
        Assert.Throws<ArgumentException>(() => dict.Add(validValues[0]));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestRemove(bool testICollectionRemove)
    {
        var valueToRemove = new KeyValuePair<string, int>("7", 2);
        var valueToKeep = new KeyValuePair<string, int>("8", 5);
        var dict = new HashedLinkedList<string, int>
        {
            valueToKeep,
            valueToRemove
        };

        // verify item to remove is in dictionary before removing it
        Assert.True(
            dict.ContainsKey(valueToRemove.Key),
            $"Dictionary should contain item with key '{valueToRemove.Key}'"
        );
        Assert.True(dict.Count == 2, "Dictionary should have 2 items");

        // now remove item
        bool removed = testICollectionRemove
            ? dict.Remove(valueToRemove)
            : dict.Remove(valueToRemove.Key);

        // verify item no longer in dictionary
        Assert.True(removed);
        Assert.True(dict.Count == 1, "Dictionary count should decrement by 1 after removing item");
        Assert.False(
            dict.ContainsKey(valueToRemove.Key),
            $"Dictionary should not contain item with key '{valueToRemove.Key}' after removing"
        );
        Assert.True(
            dict.ContainsKey(valueToKeep.Key),
            $"Dictionary should still contain item with key '{valueToKeep.Key}'"
        );

        // try removing the same key again
        removed = testICollectionRemove
            ? dict.Remove(valueToRemove)
            : dict.Remove(valueToRemove.Key);
        Assert.False(removed);
    }

    [Fact]
    public void TestRemoveFirstAndLast()
    {
        var items = new List<KeyValuePair<string, string>>
        {
            new("1", "4"),
            new("9", "1"),
            new ("3", "0"),
            new ("35543", "1")
        };
        var dict = new HashedLinkedList<string, string>();
        // now add the items to the dictionary
        items.ForEach(item => dict.Add(item));
        Assert.True(items.Count == dict.Count);

        // remove first item
        bool removed = dict.RemoveFirst();
        Assert.True(removed);
        Assert.False(
            dict.ContainsKey(items.First().Key),
            $"Dictionary should no longer contain item with key {items.First().Key}"
        );
        Assert.True(dict.Count == (items.Count-1), "Dictionary should contain one less item");

        // remove second item
        removed = dict.RemoveLast();
        Assert.True(removed);
        Assert.False(
            dict.ContainsKey(items.Last().Key),
            $"Dictionary should no longer contain item with key {items.Last().Key}"
        );
        Assert.True(dict.Count == (items.Count-2), "Dictionary should contain one less item");
    }

    [Fact]
    public void TestRemoveFirstAndLastOnEmpty()
    {
        var dict = new HashedLinkedList<string, string>();

        // verify it's safe to call remove methods when the first and last nodes are null
        bool removedFirst = dict.RemoveFirst();
        bool removedLast = dict.RemoveLast();
        Assert.False(removedFirst);
        Assert.False(removedLast);
    }

    [Fact]
    public void TestClear()
    {
        var validKey = "1";
        var dict = new HashedLinkedList<string, int>();
        dict.AddFirst(validKey, 1);
        dict.AddFirst("2", 2);

        Assert.True(dict.Count == 2, "Dictionary should have 2 items");
        dict.Clear();
        Assert.Empty(dict);
        Assert.True(dict.Count == 0);
        Assert.False(
            dict.ContainsKey(validKey),
            $"Dictionary should not contain item with key '{validKey}' after clearing dictionary"
        );
    }

    [Fact]
    public void TestIndexer()
    {
        var key1 = "key";
        var invalidKey = "keynotfound";
        var dict = new HashedLinkedList<string, int>();

        Assert.Empty(dict);

        // add a value for key 1 and get the value back using the indexer
        var valueToTest = 1;
        var node = dict.AddLast(key1, valueToTest);
        Assert.True(ReferenceEquals(dict[key1], node));
        Assert.True(dict[key1].Value.Value == valueToTest);
        Assert.True(dict.Count == 1);

        // update value for key 1
        valueToTest = 2;
        node = dict[key1];
        node.Value = new KeyValuePair<string, int>(key1, valueToTest);
        Assert.True(ReferenceEquals(dict[key1], node));
        Assert.True(dict[key1].Value.Value == valueToTest);
        Assert.True(dict.Count == 1);

        // try accessing a nonexisting key and verify exception is raised
        Assert.Throws<KeyNotFoundException>(() => dict[invalidKey]);
    }

    [Fact]
    public void TestSequentialOrder()
    {
        var testList = new List<KeyValuePair<string, int>>
        {
            new("one", 1),
            new("two", 2),
            new("three", 3),
            new("four", 4),
            new("five", 5),
            new("six", 6)
        };
        var dict = new HashedLinkedList<string, int>();
        var testIndex = testList.Count / 2;

        // add the items to the dictionary
        testList.ForEach(entry => dict.AddLast(entry));
        Assert.Equal(testList, dict);

        // move an item to the end
        var itemToMove = testList[testIndex];
        testList.RemoveAt(testIndex);
        testList.Add(itemToMove);
        // move the item in the dictionary too
        var node = dict[itemToMove.Key];
        dict.Remove(node);
        dict.AddLast(node);
        Assert.Equal(testList, dict);

        // move an item from the middle to the start
        itemToMove = testList[testIndex];
        testList.RemoveAt(testIndex);
        testList.Insert(0, itemToMove);
        // move the item in the dictionary too
        node = dict[itemToMove.Key];
        dict.Remove(node);
        dict.AddFirst(node);
        Assert.Equal(testList, dict);

        // remove last item
        testList.RemoveAt(testList.Count-1);
        dict.RemoveLast();
        Assert.Equal(testList, dict);

        // remove first item
        testList.RemoveAt(0);
        dict.RemoveFirst();
        Assert.Equal(testList, dict);
    }

    [Fact]
    public void TestConstructor()
    {
        var testList = new List<KeyValuePair<string, int>>
        {
            new("one", 1),
            new("two", 2),
            new("three", 3),
            new("four", 4),
            new("five", 5),
            new("six", 6)
        };
        var dict = new HashedLinkedList<string, int>()
        {
            testList[0],
            testList[1],
            testList[2],
            testList[3],
            testList[4],
            testList[5],
        };

        Assert.Equal(testList, dict);
    }
}
