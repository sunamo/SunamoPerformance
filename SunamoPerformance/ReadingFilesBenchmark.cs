namespace SunamoPerformance;

/// <summary>
/// Manual benchmark comparing different ways of reading a text file with TF.
/// Ported from the obsolete sunamo.performance console app, without the
/// interactive Console.ReadLine() blocking call and without hardcoded paths.
/// </summary>
public static class ReadingFilesBenchmark
{
    /// <summary>Measures how long the synchronous TF.ReadAllTextSync takes to read the given file and returns the elapsed milliseconds.</summary>
    public static long MeasureReadAllTextSync(string filePath)
    {
        StopwatchStatic.Start();
        BenchmarkFileHelper.ReadAllTextSync(filePath);
        return StopwatchStatic.StopAndPrintElapsed("TF.ReadAllTextSync");
    }

    /// <summary>Measures how long the asynchronous TF.ReadAllText takes to read the given file and returns the elapsed milliseconds.</summary>
    public static async Task<long> MeasureReadAllTextAsync(string filePath)
    {
        StopwatchStatic.Start();
        await BenchmarkFileHelper.ReadAllText(filePath);
        return StopwatchStatic.StopAndPrintElapsed("TF.ReadAllText (async)");
    }

    /// <summary>Measures how long the asynchronous TF.ReadAllLines takes to read the given file line by line and returns the elapsed milliseconds.</summary>
    public static async Task<long> MeasureReadAllLinesAsync(string filePath)
    {
        StopwatchStatic.Start();
        await BenchmarkFileHelper.ReadAllLines(filePath);
        return StopwatchStatic.StopAndPrintElapsed("TF.ReadAllLines (async)");
    }

    /// <summary>Creates a sibling copy path for the given file by inserting a suffix between the file name and its extension, using FS.InsertBetweenFileNameAndExtension.</summary>
    public static string BuildSiblingCopyPath(string filePath, string suffix)
    {
        return BenchmarkFileHelper.InsertBetweenFileNameAndExtension(filePath, suffix);
    }

    /// <summary>Runs all three reading benchmarks (sync, async, async lines) against the same file and returns their elapsed milliseconds keyed by benchmark name.</summary>
    public static async Task<Dictionary<string, long>> MeasureAllAsync(string filePath)
    {
        var results = new Dictionary<string, long>
        {
            ["TF.ReadAllTextSync"] = MeasureReadAllTextSync(filePath),
            ["TF.ReadAllText (async)"] = await MeasureReadAllTextAsync(filePath),
            ["TF.ReadAllLines (async)"] = await MeasureReadAllLinesAsync(filePath)
        };
        return results;
    }
}
