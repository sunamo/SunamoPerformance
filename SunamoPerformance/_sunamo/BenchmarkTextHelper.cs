namespace SunamoPerformance._sunamo;

/// <summary>
/// Text helpers used by the benchmarks (reduced copies of SHGetLines, SHSplit and SHReplace to keep this package flat).
/// </summary>
internal static class BenchmarkTextHelper
{
    /// <summary>
    /// Splits a text into lines, handling \r\n, \n\r, \r and \n newlines.
    /// </summary>
    /// <param name="text">The text to split.</param>
    internal static List<string> GetLines(string text)
    {
        return text.Split(new[] { "\r\n", "\n\r", "\r", "\n" }, StringSplitOptions.None).ToList();
    }

    /// <summary>
    /// Splits a text by the delimiter, trims the parts and drops the empty ones.
    /// </summary>
    /// <param name="text">The text to split.</param>
    /// <param name="delimiter">The delimiter string.</param>
    internal static List<string> Split(string text, string delimiter)
    {
        return text.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim())
            .Where(part => part != string.Empty)
            .ToList();
    }

    /// <summary>
    /// Parses text in the "search->replacement" per-line format into the searched lines and the replacement lines.
    /// </summary>
    /// <param name="text">Text with one "search->replacement" pair per line, or a plain search text.</param>
    internal static Tuple<string, string> SplitFromReplaceManyFormat(string text)
    {
        var replacementBuilder = new StringBuilder();
        var searchBuilder = new StringBuilder();
        if (text.Contains("->"))
        {
            var lines = GetLines(text).ConvertAll(element => element.Trim());
            foreach (var item in lines)
            {
                var parts = Split(item, "->");
                searchBuilder.AppendLine(parts[0]);
                replacementBuilder.AppendLine(parts[1]);
            }
        }
        else
        {
            searchBuilder.AppendLine(text);
        }

        return new Tuple<string, string>(searchBuilder.ToString(), replacementBuilder.ToString());
    }

    /// <summary>
    /// Replaces every replaceFrom[i] in the content with replaceTo[i].
    /// </summary>
    /// <param name="replaceFrom">Strings to search for.</param>
    /// <param name="replaceTo">Replacement strings, same count as replaceFrom.</param>
    /// <param name="content">The content to process.</param>
    internal static string ReplaceAll(IList<string> replaceFrom, IList<string> replaceTo, string content)
    {
        for (var index = 0; index < replaceFrom.Count; index++)
            content = content.Replace(replaceFrom[index], replaceTo[index]);
        return content;
    }
}
