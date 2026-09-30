namespace SunamoPerformance;

/// <summary>
/// Manual benchmark comparing different ways of writing a text file with TF.
/// Ported from the obsolete sunamo.performance console app, without the
/// interactive Console.ReadLine() blocking call and without hardcoded paths
/// (the original used a hardcoded "D:\_Test\..." path).
/// </summary>
public static class WritingFilesBenchmark
{
    /// <summary>Builds a test content string of the given length, filled with the given repeated character (defaults to '@').</summary>
    public static string BuildTestContent(int length, char fillCharacter = '@')
    {
        return string.Empty.PadLeft(length, fillCharacter);
    }

    /// <summary>Measures how long the synchronous TF.SaveFile takes to write the given content to the given file and returns the elapsed milliseconds.</summary>
    public static long MeasureSaveFile(string filePath, string content)
    {
        StopwatchStatic.Start();
        BenchmarkFileHelper.SaveFile(content, filePath);
        return StopwatchStatic.StopAndPrintElapsed("TF.SaveFile");
    }

    /// <summary>Measures how long the asynchronous TF.WriteAllText takes to write the given content to the given file and returns the elapsed milliseconds.</summary>
    public static async Task<long> MeasureWriteAllTextAsync(string filePath, string content)
    {
        StopwatchStatic.Start();
        await BenchmarkFileHelper.WriteAllText(filePath, content);
        return StopwatchStatic.StopAndPrintElapsed("TF.WriteAllText (async)");
    }

    /// <summary>Runs both writing benchmarks (TF.SaveFile and TF.WriteAllText) against fresh temp files with the given content length and returns their elapsed milliseconds keyed by benchmark name.</summary>
    public static async Task<Dictionary<string, long>> MeasureAllAsync(int contentLength)
    {
        var content = BuildTestContent(contentLength);
        var saveFilePath = Path.Combine(Path.GetTempPath(), $"SunamoPerformance_SaveFile_{Guid.NewGuid():N}.txt");
        var writeAllTextPath = Path.Combine(Path.GetTempPath(), $"SunamoPerformance_WriteAllText_{Guid.NewGuid():N}.txt");

        var results = new Dictionary<string, long>
        {
            ["TF.SaveFile"] = MeasureSaveFile(saveFilePath, content),
            ["TF.WriteAllText (async)"] = await MeasureWriteAllTextAsync(writeAllTextPath, content)
        };

        File.Delete(saveFilePath);
        File.Delete(writeAllTextPath);

        return results;
    }
}
