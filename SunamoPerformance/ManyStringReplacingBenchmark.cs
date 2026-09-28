namespace SunamoPerformance;

/// <summary>
/// Manual benchmark comparing bulk string replacing implementations.
/// Ported from the obsolete sunamo.performance console app, without the
/// interactive Console.ReadLine() blocking call and without hardcoded paths.
/// </summary>
public static class ManyStringReplacingBenchmark
{
    /// <summary>Parses a "replace many" formatted text (lines of "from-&gt;to") into two parallel lists: values to replace from and values to replace to, with trailing empty lines removed.</summary>
    public static (List<string> ReplaceFrom, List<string> ReplaceTo) ParseReplacePairs(string replacePairsText)
    {
        var splitPairs = SHSplit.SplitFromReplaceManyFormat(replacePairsText);
        var replaceFrom = SHGetLines.GetLines(splitPairs.Item1).Where(value => !string.IsNullOrEmpty(value)).ToList();
        var replaceTo = SHGetLines.GetLines(splitPairs.Item2).Where(value => !string.IsNullOrEmpty(value)).ToList();
        return (replaceFrom, replaceTo);
    }

    /// <summary>Measures how long SHReplace.ReplaceAll3 takes to apply all replace pairs to the given content and returns the elapsed milliseconds.</summary>
    public static long MeasureReplaceAll3(string content, IList<string> replaceFrom, IList<string> replaceTo)
    {
        StopwatchStatic.Start();
        SHReplace.ReplaceAll3(replaceFrom, replaceTo, false, content);
        return StopwatchStatic.StopAndPrintElapsed("SHReplace.ReplaceAll3");
    }

    /// <summary>Reads the given file synchronously and measures how long SHReplace.ReplaceAll3 takes to apply the replace pairs to its content.</summary>
    public static long MeasureReplaceAll3ForFile(string filePath, IList<string> replaceFrom, IList<string> replaceTo)
    {
        var content = TF.ReadAllTextSync(filePath);
        return MeasureReplaceAll3(content, replaceFrom, replaceTo);
    }
}
