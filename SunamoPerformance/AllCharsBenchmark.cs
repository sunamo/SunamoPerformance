namespace SunamoPerformance;

/// <summary>
/// Manual benchmark measuring how long it takes to look up characters inside
/// a collection of "all interesting characters" (letters, digits, special
/// characters). Self-contained rewrite of the original AllChars partial
/// class, which referenced fields (specialChars/specialChars2) defined
/// elsewhere in the original private Sunamo library and not available here.
/// </summary>
public static class AllCharsBenchmark
{
    private static readonly List<char> SpecialChars = new List<char>
    {
        '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '=', '+',
        '[', ']', '{', '}', ';', ':', '\'', '"', ',', '.', '<', '>', '/', '?', '\\', '|', '`', '~'
    };

    /// <summary>Returns a copy of the list of special characters used as test data for lookup benchmarks.</summary>
    public static List<char> GetSpecialChars()
    {
        return new List<char>(SpecialChars);
    }

    /// <summary>Returns a list of all lowercase and uppercase ASCII letters plus digits 0-9, usable as test data for lookup benchmarks.</summary>
    public static List<char> GetAlphanumericChars()
    {
        var alphanumericChars = new List<char>();
        for (var letter = 'a'; letter <= 'z'; letter++) alphanumericChars.Add(letter);
        for (var letter = 'A'; letter <= 'Z'; letter++) alphanumericChars.Add(letter);
        for (var digit = '0'; digit <= '9'; digit++) alphanumericChars.Add(digit);
        return alphanumericChars;
    }

    /// <summary>Measures how long it takes to look up each of the given characters inside the special-characters list using List.Contains and returns the elapsed milliseconds.</summary>
    public static long MeasureSpecialCharsContainsLookup(IEnumerable<char> charsToSearch)
    {
        StopwatchStatic.Start();
        foreach (var character in charsToSearch)
        {
            SpecialChars.Contains(character);
        }
        return StopwatchStatic.StopAndPrintElapsed("AllChars special chars Contains lookup");
    }
}
