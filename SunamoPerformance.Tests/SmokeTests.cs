namespace SunamoPerformance.Tests;

/// <summary>
/// Smoke tests verifying that every migrated benchmark method runs to
/// completion without throwing, using small input sizes.
/// </summary>
public class SmokeTests
{
    /// <summary>Verifies ManyStringReplacingBenchmark.ParseReplacePairs and MeasureReplaceAll3 run without exceptions.</summary>
    [Fact]
    public void ManyStringReplacingBenchmark_ParsesPairsAndMeasuresReplaceAll3()
    {
        var (replaceFrom, replaceTo) = ManyStringReplacingBenchmark.ParseReplacePairs("foo->bar\nbaz->qux");

        Assert.Contains("foo", replaceFrom);
        Assert.Contains("baz", replaceFrom);
        Assert.Contains("bar", replaceTo);
        Assert.Contains("qux", replaceTo);

        var elapsedMilliseconds = ManyStringReplacingBenchmark.MeasureReplaceAll3("foo baz content", replaceFrom, replaceTo);

        Assert.True(elapsedMilliseconds >= 0);
    }

    /// <summary>Verifies ManyStringReplacingBenchmark.MeasureReplaceAll3ForFile reads a real file and measures the replace without exceptions.</summary>
    [Fact]
    public void ManyStringReplacingBenchmark_MeasuresReplaceAll3ForFile()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"SunamoPerformanceTests_{Guid.NewGuid():N}.txt");
        File.WriteAllText(filePath, "foo baz content");
        try
        {
            var elapsedMilliseconds = ManyStringReplacingBenchmark.MeasureReplaceAll3ForFile(filePath, new List<string> { "foo" }, new List<string> { "bar" });
            Assert.True(elapsedMilliseconds >= 0);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    /// <summary>Verifies every ReadingFilesBenchmark method runs against a real temp file without exceptions.</summary>
    [Fact]
    public async Task ReadingFilesBenchmark_MeasuresAllReadingStrategies()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"SunamoPerformanceTests_{Guid.NewGuid():N}.txt");
        File.WriteAllText(filePath, "line one\nline two\nline three");
        try
        {
            var results = await ReadingFilesBenchmark.MeasureAllAsync(filePath);

            Assert.Equal(3, results.Count);
            Assert.All(results.Values, elapsedMilliseconds => Assert.True(elapsedMilliseconds >= 0));

            var siblingCopyPath = ReadingFilesBenchmark.BuildSiblingCopyPath(filePath, "_copy");
            Assert.Contains("_copy", siblingCopyPath);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    /// <summary>Verifies every WritingFilesBenchmark method runs against temp files without exceptions and cleans up after itself.</summary>
    [Fact]
    public async Task WritingFilesBenchmark_MeasuresAllWritingStrategies()
    {
        var results = await WritingFilesBenchmark.MeasureAllAsync(contentLength: 1000);

        Assert.Equal(2, results.Count);
        Assert.All(results.Values, elapsedMilliseconds => Assert.True(elapsedMilliseconds >= 0));
    }

    /// <summary>Verifies WritingFilesBenchmark.BuildTestContent produces content of the requested length filled with the given character.</summary>
    [Fact]
    public void WritingFilesBenchmark_BuildsTestContentOfRequestedLength()
    {
        var content = WritingFilesBenchmark.BuildTestContent(50, '#');

        Assert.Equal(50, content.Length);
        Assert.All(content, character => Assert.Equal('#', character));
    }

    /// <summary>Verifies every FastStringLookupBenchmark.MeasureAll benchmark runs against small generated data without exceptions.</summary>
    [Fact]
    public void FastStringLookupBenchmark_MeasuresAllLookupStrategies()
    {
        var results = FastStringLookupBenchmark.MeasureAll(count: 50, length: 8);

        Assert.Equal(7, results.Count);
        Assert.All(results.Values, elapsedMilliseconds => Assert.True(elapsedMilliseconds >= 0));
    }

    /// <summary>Verifies FastStringLookupBenchmark.GenerateRandomStrings returns the requested number of non-empty strings.</summary>
    [Fact]
    public void FastStringLookupBenchmark_GeneratesRequestedNumberOfRandomStrings()
    {
        var randomStrings = FastStringLookupBenchmark.GenerateRandomStrings(count: 10, length: 12);

        Assert.Equal(10, randomStrings.Count);
        Assert.All(randomStrings, value => Assert.False(string.IsNullOrEmpty(value)));
    }

    /// <summary>Verifies AllCharsBenchmark returns the expected character sets and measures lookups without exceptions.</summary>
    [Fact]
    public void AllCharsBenchmark_MeasuresSpecialCharsLookup()
    {
        var specialChars = AllCharsBenchmark.GetSpecialChars();
        var alphanumericChars = AllCharsBenchmark.GetAlphanumericChars();

        Assert.NotEmpty(specialChars);
        Assert.Equal(62, alphanumericChars.Count);

        var elapsedMilliseconds = AllCharsBenchmark.MeasureSpecialCharsContainsLookup(alphanumericChars.Take(5));

        Assert.True(elapsedMilliseconds >= 0);
    }

    /// <summary>Verifies WrapperStringObject.ToString returns the wrapped value.</summary>
    [Fact]
    public void WrapperStringObject_ToStringReturnsWrappedValue()
    {
        var wrapper = new WrapperStringObject("hello");

        Assert.Equal("hello", wrapper.ToString());
    }

    /// <summary>Verifies StringReplacingBenchmark.MeasureAll runs every implementation against small test cases without exceptions.</summary>
    [Fact]
    public void StringReplacingBenchmark_MeasuresAllImplementations()
    {
        var testCases = new[] { (ReplacementCount: 5, InputLength: 200) };

        var results = StringReplacingBenchmark.MeasureAll(testCases);

        Assert.Equal(4, results.Count);
        Assert.All(results.Values, averageMilliseconds => Assert.True(averageMilliseconds >= 0));
    }
}
