namespace Cmg.Dotnet.XCollections.Test;

using Xunit;

using Cmg.Dotnet.XCollections;

public class SequentialDictionaryTest
{

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    public void TestContainsKey(int key)
    {
        var otherKey = key+4;
        var invalidKey = key+2;
        var dict = new SequentialDictionary<int, int>
        {
            [key] = 1,
            [otherKey] = 1,
        };

        Assert.True(dict.ContainsKey(key), $"Dictionary should contain key '{key}'");
        Assert.True(dict.ContainsKey(otherKey), $"Dictionary should contain key '{otherKey}'");
        Assert.False(dict.ContainsKey(invalidKey), $"Dictionary should not contain key '{invalidKey}'");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestAddAndContains(bool testICollectionAdd)
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
        var dict = new SequentialDictionary<string, int>();
        foreach(var validValue in validValues)
        {
            if (testICollectionAdd)
            {
                dict.Add(validValue);
            }
            else
            {
                dict.Add(validValue.Key, validValue.Value);
            }
        }

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
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestRemove(bool testICollectionRemove)
    {
        var valueToRemove = new KeyValuePair<string, int>("7", 2);
        var valueToKeep = new KeyValuePair<string, int>("8", 5);
        var dict = new SequentialDictionary<string, int>
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
        if (testICollectionRemove)
        {
            dict.Remove(valueToRemove);
        }
        else
        {
            dict.Remove(valueToRemove.Key);
        }

        // verify item no longer in dictionary
        Assert.True(dict.Count == 1, "Dictionary count should decrement by 1 after removing item");
        Assert.False(
            dict.ContainsKey(valueToRemove.Key),
            $"Dictionary should not contain item with key '{valueToRemove.Key}' after removing"
        );
        Assert.True(
            dict.ContainsKey(valueToKeep.Key),
            $"Dictionary should still contain item with key '{valueToKeep.Key}'"
        );
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
        var dict = new SequentialDictionary<string, string>();
        foreach (var item in items)
        {
            dict.Add(item);
        }
        Assert.True(items.Count == dict.Count);

        // remove first item
        dict.RemoveFirst();
        Assert.False(
            dict.ContainsKey(items.First().Key),
            $"Dictionary should no longer contain item with key {items.First().Key}"
        );
        Assert.True(dict.Count == (items.Count-1), "Dictionary should contain one less item");

        // remove second item
        dict.RemoveLast();
        Assert.False(
            dict.ContainsKey(items.Last().Key),
            $"Dictionary should no longer contain item with key {items.Last().Key}"
        );
        Assert.True(dict.Count == (items.Count-2), "Dictionary should contain one less item");
    }

    [Fact]
    public void TestClear()
    {
        var validKey = "1";
        var dict = new SequentialDictionary<string, int>()
        {
            [validKey] = 1,
            ["2"] = 2,
        };

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
        var key2 = "key2";
        var invalidKey = "keynotfound";
        var dict = new SequentialDictionary<string, int>();

        Assert.Empty(dict);

        // set a value for key 1
        var valueToTest = 1;
        dict[key1] = valueToTest;
        Assert.True(dict[key1] == valueToTest);
        Assert.True(dict.Count == 1);

        // update value for key 1 and verify count is same
        valueToTest = 2;
        dict[key1] = valueToTest;
        Assert.True(dict[key1] == valueToTest);
        Assert.True(dict.Count == 1);

        // add a new value
        valueToTest = 5;
        dict[key2] = valueToTest;
        Assert.True(dict[key2] == valueToTest);
        Assert.True(dict.Count == 2);

        // try accessing a nonexisting key and verify exception is raised
        Assert.Throws<KeyNotFoundException>(() => dict[invalidKey]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TestSequentialOrder(bool useIndexer)
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
        var dict = new SequentialDictionary<string, int>();
        var testIndex = testList.Count / 2;

        // add the items to the dictionary
        foreach (var item in testList)
        {
            if (useIndexer)
            {
                dict[item.Key] = item.Value;
            }
            else
            {
                dict.Add(item);
            }
        }

        Assert.Equal(testList, dict);
        Assert.Equal(testList.Select(x => x.Key), dict.Keys);
        Assert.Equal(testList.Select(x => x.Value), dict.Values);

        // move an item to the end
        var itemToMove = testList[testIndex];
        testList.RemoveAt(testIndex);
        testList.Add(itemToMove);
        // move the item in the dictionary too
        dict.MoveToLast(itemToMove.Key);

        Assert.Equal(testList, dict);
        Assert.Equal(testList.Select(x => x.Key), dict.Keys);
        Assert.Equal(testList.Select(x => x.Value), dict.Values);

        // move an item from the middle to the start
        itemToMove = testList[testIndex];
        testList.RemoveAt(testIndex);
        testList.Insert(0, itemToMove);
        // move the item in the dictionary too
        dict.MoveToFirst(itemToMove.Key);

        Assert.Equal(testList, dict);
        Assert.Equal(testList.Select(x => x.Key), dict.Keys);
        Assert.Equal(testList.Select(x => x.Value), dict.Values);

        // remove last item
        testList.RemoveAt(testList.Count-1);
        dict.RemoveLast();

        Assert.Equal(testList, dict);
        Assert.Equal(testList.Select(x => x.Key), dict.Keys);
        Assert.Equal(testList.Select(x => x.Value), dict.Values);

        // remove first item
        testList.RemoveAt(0);
        dict.RemoveFirst();

        Assert.Equal(testList, dict);
        Assert.Equal(testList.Select(x => x.Key), dict.Keys);
        Assert.Equal(testList.Select(x => x.Value), dict.Values);
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
        var dict = new SequentialDictionary<string, int>()
        {
            testList[0],
            testList[1],
            testList[2],
            testList[3],
            testList[4],
            testList[5],
        };

        Assert.Equal(testList, dict);
        Assert.Equal(testList.Select(x => x.Key), dict.Keys);
        Assert.Equal(testList.Select(x => x.Value), dict.Values);
    }
}
