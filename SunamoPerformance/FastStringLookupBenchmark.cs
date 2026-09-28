namespace SunamoPerformance;

/// <summary>
/// Manual benchmark comparing the lookup speed of different collection
/// types for strings (HashSet, Dictionary key, List.Contains, linear array
/// scan, SortedList, ConcurrentDictionary). Ported from the obsolete
/// sunamo.performance console app, without the interactive Console.ReadLine()
/// blocking call, without hardcoded output paths and without the
/// Thread.Sleep "cooldown" calls between runs.
/// </summary>
public static class FastStringLookupBenchmark
{
    /// <summary>Generates the given number of random strings of the given length, usable as test data for lookup benchmarks.</summary>
    public static List<string> GenerateRandomStrings(int count, int length)
    {
        var randomStrings = new List<string>(count);
        for (var index = 0; index < count; index++)
        {
            randomStrings.Add(RandomStringHelper.RandomString(length, index % 5));
        }
        return randomStrings;
    }

    /// <summary>Measures how long it takes to look up every value from valuesToSearch inside a HashSet built from valuesToStore and returns the elapsed milliseconds.</summary>
    public static long MeasureHashSetLookup(IEnumerable<string> valuesToStore, IEnumerable<string> valuesToSearch)
    {
        var hashSet = new HashSet<string>(valuesToStore);
        StopwatchStatic.Start();
        foreach (var value in valuesToSearch)
        {
            hashSet.Contains(value);
        }
        return StopwatchStatic.StopAndPrintElapsed("HashSet<string>.Contains lookup");
    }

    /// <summary>Measures how long it takes to look up every value from valuesToSearch inside a HashSet of WrapperStringObject built from valuesToStore and returns the elapsed milliseconds. Because WrapperStringObject does not override equality, this effectively benchmarks reference-equality lookups.</summary>
    public static long MeasureHashSetOfWrapperObjectsLookup(IEnumerable<string> valuesToStore, IEnumerable<string> valuesToSearch)
    {
        var hashSet = new HashSet<WrapperStringObject>(valuesToStore.Select(value => new WrapperStringObject(value)));
        var wrappedValuesToSearch = valuesToSearch.Select(value => new WrapperStringObject(value)).ToList();
        StopwatchStatic.Start();
        foreach (var wrappedValue in wrappedValuesToSearch)
        {
            hashSet.Contains(wrappedValue);
        }
        return StopwatchStatic.StopAndPrintElapsed("HashSet<WrapperStringObject>.Contains lookup");
    }

    /// <summary>Measures how long it takes to look up every value from valuesToSearch by key inside a Dictionary built from valuesToStore and returns the elapsed milliseconds.</summary>
    public static long MeasureDictionaryKeyLookup(IEnumerable<string> valuesToStore, IEnumerable<string> valuesToSearch)
    {
        var dictionary = new Dictionary<string, string>();
        foreach (var value in valuesToStore)
        {
            dictionary[value] = value;
        }

        StopwatchStatic.Start();
        foreach (var value in valuesToSearch)
        {
            dictionary.ContainsKey(value);
        }
        return StopwatchStatic.StopAndPrintElapsed("Dictionary<string,string>.ContainsKey lookup");
    }

    /// <summary>Measures how long it takes to look up every value from valuesToSearch by key inside a ConcurrentDictionary built from valuesToStore and returns the elapsed milliseconds.</summary>
    public static long MeasureConcurrentDictionaryKeyLookup(IEnumerable<string> valuesToStore, IEnumerable<string> valuesToSearch)
    {
        var concurrentDictionary = new ConcurrentDictionary<string, string>();
        foreach (var value in valuesToStore)
        {
            concurrentDictionary[value] = value;
        }

        StopwatchStatic.Start();
        foreach (var value in valuesToSearch)
        {
            concurrentDictionary.ContainsKey(value);
        }
        return StopwatchStatic.StopAndPrintElapsed("ConcurrentDictionary<string,string>.ContainsKey lookup");
    }

    /// <summary>Measures how long it takes to look up every value from valuesToSearch inside a List using List.Contains and returns the elapsed milliseconds.</summary>
    public static long MeasureListContainsLookup(IEnumerable<string> valuesToStore, IEnumerable<string> valuesToSearch)
    {
        var list = new List<string>(valuesToStore);
        StopwatchStatic.Start();
        foreach (var value in valuesToSearch)
        {
            list.Contains(value);
        }
        return StopwatchStatic.StopAndPrintElapsed("List<string>.Contains lookup");
    }

    /// <summary>Measures how long it takes to look up every value from valuesToSearch inside a sorted List using BinarySearch and returns the elapsed milliseconds.</summary>
    public static long MeasureListBinarySearchLookup(IEnumerable<string> valuesToStore, IEnumerable<string> valuesToSearch)
    {
        var sortedList = new List<string>(valuesToStore);
        sortedList.Sort();
        StopwatchStatic.Start();
        foreach (var value in valuesToSearch)
        {
            sortedList.BinarySearch(value);
        }
        return StopwatchStatic.StopAndPrintElapsed("List<string>.BinarySearch lookup");
    }

    /// <summary>Measures how long it takes to look up every value from valuesToSearch inside a plain array using a linear scan and returns the elapsed milliseconds.</summary>
    public static long MeasureArrayLinearLookup(IEnumerable<string> valuesToStore, IEnumerable<string> valuesToSearch)
    {
        var array = valuesToStore.ToArray();
        StopwatchStatic.Start();
        foreach (var value in valuesToSearch)
        {
            Array.IndexOf(array, value);
        }
        return StopwatchStatic.StopAndPrintElapsed("Array linear scan lookup");
    }

    /// <summary>Runs every lookup benchmark against the same freshly generated test data and returns the elapsed milliseconds keyed by benchmark name, ordered from fastest to slowest.</summary>
    public static Dictionary<string, long> MeasureAll(int count, int length)
    {
        var valuesToStore = GenerateRandomStrings(count, length);
        var valuesToSearch = GenerateRandomStrings(count, length);

        var results = new Dictionary<string, long>
        {
            ["HashSet<string>"] = MeasureHashSetLookup(valuesToStore, valuesToSearch),
            ["HashSet<WrapperStringObject>"] = MeasureHashSetOfWrapperObjectsLookup(valuesToStore, valuesToSearch),
            ["Dictionary key"] = MeasureDictionaryKeyLookup(valuesToStore, valuesToSearch),
            ["ConcurrentDictionary key"] = MeasureConcurrentDictionaryKeyLookup(valuesToStore, valuesToSearch),
            ["List.Contains"] = MeasureListContainsLookup(valuesToStore, valuesToSearch),
            ["List.BinarySearch"] = MeasureListBinarySearchLookup(valuesToStore, valuesToSearch),
            ["Array linear scan"] = MeasureArrayLinearLookup(valuesToStore, valuesToSearch)
        };

        return results.OrderBy(entry => entry.Value).ToDictionary(entry => entry.Key, entry => entry.Value);
    }
}
