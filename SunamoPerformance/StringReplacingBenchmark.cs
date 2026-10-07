namespace SunamoPerformance;

/// <summary>
/// Manual benchmark comparing several string-replace implementations (simple, parallel,
/// substring-based, unsafe pointer-based). Ported from the obsolete sunamo.performance
/// console app, without the blocking Console.ReadKey() call.
/// </summary>
public static class StringReplacingBenchmark
{
    private static readonly Random Seed = new Random(42);

    /// <summary>Builds a random input string of the given length using only the characters A, B and C.</summary>
    public static string BuildRandomInputString(int length)
    {
        var input = new StringBuilder();
        for (var index = 0; index < length; index++)
        {
            var randomNumber = Seed.Next(0, 3);
            var character = randomNumber == 0 ? 'A' : randomNumber == 1 ? 'B' : 'C';
            input.Append(character);
        }
        return input.ToString();
    }

    /// <summary>Builds a list of random 2-character replacement strings.</summary>
    public static List<string> BuildRandomReplacements(int replacementsCount)
    {
        var replacements = new List<string>();
        for (var index = 0; index < replacementsCount; index++)
        {
            var randomNumber = Seed.Next(0, 3);
            var replaceIteration = randomNumber == 0 ? "AB" : randomNumber == 1 ? "BC" : "CD";
            replacements.Add(replaceIteration);
        }
        return replacements;
    }

    private static long Measure(string input, string replace, string[] replacements, Action<string, string, string[]> implementation)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();

        var stopWatch = Stopwatch.StartNew();
        implementation(input, replace, replacements);
        stopWatch.Stop();

        return stopWatch.ElapsedMilliseconds;
    }

    private static void SimpleImplementation(string input, string replace, string[] replacements)
    {
        foreach (var replaceBy in replacements)
        {
            var result = input.Replace(replace, replacements[0]);
        }
    }

    private static void SimpleParallelImplementation(string input, string replace, string[] replacements)
    {
        var rangePartitioner = Partitioner.Create(0, replacements.Length);

        Parallel.ForEach(rangePartitioner, (range, loopState) =>
        {
            for (var replacementIndex = range.Item1; replacementIndex < range.Item2; replacementIndex++)
            {
                var result = input.Replace(replace, replacements[replacementIndex]);
            }
        });
    }

    private static void ParallelSubstringImplementation(string input, string replace, string[] replaceBy)
    {
        var startingPosition = 0;
        var indexes = new List<int>();

        int currentPosition;
        while ((currentPosition = input.IndexOf(replace, startingPosition)) >= 0)
        {
            indexes.Add(currentPosition);
            startingPosition = currentPosition + 1;
        }

        var replaceByPartitioner = Partitioner.Create(0, replaceBy.Length);
        var rangePartitioner = Partitioner.Create(0, indexes.Count);

        Parallel.ForEach(replaceByPartitioner, (outerRange, outerLoopState) =>
        {
            for (var replaceByIndex = outerRange.Item1; replaceByIndex < outerRange.Item2; replaceByIndex++)
            {
                var replaceWith = replaceBy[replaceByIndex];

                var finalSize = input.Length - (indexes.Count * replace.Length) + (indexes.Count * replaceWith.Length);
                var finalResult = new char[finalSize];

                Parallel.ForEach(rangePartitioner, (innerRange, innerLoopState) =>
                {
                    for (var matchIndex = innerRange.Item1; matchIndex < innerRange.Item2; matchIndex++)
                    {
                        var currentIndex = indexes[matchIndex];
                        var prevIndex = matchIndex > 0 ? indexes[matchIndex - 1] : -replace.Length;

                        var outputPosition = 0;
                        if (prevIndex >= 0)
                        {
                            outputPosition = prevIndex + replace.Length;
                            if (replace.Length != replaceWith.Length)
                            {
                                var offset = (replace.Length - replaceWith.Length) * matchIndex;
                                var dir = replace.Length < replaceWith.Length;
                                outputPosition = prevIndex + offset + replaceWith.Length + (dir ? 1 : -1);
                            }
                        }

                        for (var prefixIndex = prevIndex + replace.Length; prefixIndex < currentIndex; prefixIndex++)
                            finalResult[outputPosition++] = input[prefixIndex];

                        foreach (var character in replaceWith)
                            finalResult[outputPosition++] = character;

                        if (currentIndex == indexes[indexes.Count - 1])
                        {
                            for (var suffixIndex = currentIndex + replace.Length; suffixIndex < input.Length; suffixIndex++)
                                finalResult[outputPosition++] = input[suffixIndex];
                        }
                    }
                });
            }
        });
    }

    private static unsafe void FredouImplementation(string input, string replace, string[] replaceBy)
    {
        var inputLength = input.Length;
        var indexes = new List<int>();

        var len = inputLength;
        fixed (char* inputChars = input, replaceChars = replace)
        {
            while (--len > -1)
            {
                if (inputChars[len] == replaceChars[0] && inputChars[len + 1] == replaceChars[1])
                {
                    indexes.Add(len--);
                }
            }
        }

        var idx = indexes.ToArray();
        len = indexes.Count;

        Parallel.For(0, replaceBy.Length, replaceByIndex => FredouProcess(input, len, replaceBy[replaceByIndex], idx, idx.Length));
    }

    private static unsafe void FredouProcess(string input, int len, string replaceBy, int[] idx, int idxLen)
    {
        var output = new char[len];

        fixed (char* outputChars = output, inputChars = input)
        {
            for (var charIndex = 0; charIndex < len; ++charIndex)
                outputChars[charIndex] = inputChars[charIndex];

            for (var matchIndex = 0; matchIndex < idxLen; ++matchIndex)
            {
                outputChars[idx[matchIndex]] = replaceBy[0];
                outputChars[idx[matchIndex] + 1] = replaceBy[1];
            }
        }
    }

    /// <summary>
    /// Runs every implementation against a set of test cases (replacement count + input
    /// length) and returns, per implementation name, the average elapsed milliseconds.
    /// </summary>
    public static Dictionary<string, double> MeasureAll(IReadOnlyList<(int ReplacementCount, int InputLength)>? testCases = null)
    {
        testCases ??= new[]
        {
            (ReplacementCount: 500, InputLength: (int)Math.Pow(10, 6) * 2),
            (ReplacementCount: 500, InputLength: (int)Math.Pow(10, 6)),
            (ReplacementCount: 100, InputLength: (int)Math.Pow(10, 6) / 2),
            (ReplacementCount: 50, InputLength: (int)Math.Pow(10, 3)),
        };

        var implementations = new (string Name, Action<string, string, string[]> Implementation)[]
        {
            ("Simple", SimpleImplementation),
            ("SimpleParallel", SimpleParallelImplementation),
            ("ParallelSubstring", ParallelSubstringImplementation),
            ("Fredou unsafe", FredouImplementation),
        };

        const string replace = "BC";
        var result = new Dictionary<string, List<long>>();

        foreach (var testCase in testCases)
        {
            var input = BuildRandomInputString(testCase.InputLength);
            var replacements = BuildRandomReplacements(testCase.ReplacementCount).ToArray();

            foreach (var (name, implementation) in implementations)
            {
                var elapsedMilliseconds = Measure(input, replace, replacements, implementation);
                if (!result.TryGetValue(name, out var times))
                {
                    times = new List<long>();
                    result[name] = times;
                }
                times.Add(elapsedMilliseconds);
            }
        }

        return result.ToDictionary(pair => pair.Key, pair => pair.Value.Average());
    }
}
