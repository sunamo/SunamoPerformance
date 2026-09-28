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
        for (var i = 0; i < length; i++)
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
        for (var i = 0; i < replacementsCount; i++)
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
            for (var i = range.Item1; i < range.Item2; i++)
            {
                var result = input.Replace(replace, replacements[i]);
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
            for (var g = outerRange.Item1; g < outerRange.Item2; g++)
            {
                var replaceWith = replaceBy[g];

                var finalSize = input.Length - (indexes.Count * replace.Length) + (indexes.Count * replaceWith.Length);
                var finalResult = new char[finalSize];

                Parallel.ForEach(rangePartitioner, (innerRange, innerLoopState) =>
                {
                    for (var i = innerRange.Item1; i < innerRange.Item2; i++)
                    {
                        var currentIndex = indexes[i];
                        var prevIndex = i > 0 ? indexes[i - 1] : -replace.Length;

                        var n = 0;
                        if (prevIndex >= 0)
                        {
                            n = prevIndex + replace.Length;
                            if (replace.Length != replaceWith.Length)
                            {
                                var offset = (replace.Length - replaceWith.Length) * i;
                                var dir = replace.Length < replaceWith.Length;
                                n = prevIndex + offset + replaceWith.Length + (dir ? 1 : -1);
                            }
                        }

                        for (var k = prevIndex + replace.Length; k < currentIndex; k++)
                            finalResult[n++] = input[k];

                        foreach (var ch in replaceWith)
                            finalResult[n++] = ch;

                        if (currentIndex == indexes[indexes.Count - 1])
                        {
                            for (var k = currentIndex + replace.Length; k < input.Length; k++)
                                finalResult[n++] = input[k];
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
        fixed (char* i = input, r = replace)
        {
            while (--len > -1)
            {
                if (i[len] == r[0] && i[len + 1] == r[1])
                {
                    indexes.Add(len--);
                }
            }
        }

        var idx = indexes.ToArray();
        len = indexes.Count;

        Parallel.For(0, replaceBy.Length, l => FredouProcess(input, len, replaceBy[l], idx, idx.Length));
    }

    private static unsafe void FredouProcess(string input, int len, string replaceBy, int[] idx, int idxLen)
    {
        var output = new char[len];

        fixed (char* o = output, i = input)
        {
            for (var l = 0; l < len; ++l)
                o[l] = i[l];

            for (var l = 0; l < idxLen; ++l)
            {
                o[idx[l]] = replaceBy[0];
                o[idx[l] + 1] = replaceBy[1];
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
