namespace SunamoPerformance._sunamo;

/// <summary>
/// Minimal static stopwatch used by the benchmarks (reduced copy of SunamoStopwatch.StopwatchStatic to keep this package flat).
/// </summary>
internal static class StopwatchStatic
{
    private static readonly Stopwatch stopwatch = new();

    /// <summary>
    /// Resets and starts the stopwatch.
    /// </summary>
    internal static void Start()
    {
        stopwatch.Reset();
        stopwatch.Start();
    }

    /// <summary>
    /// Stops the stopwatch, prints "operationName takes Nms" to the console and returns the elapsed milliseconds.
    /// </summary>
    /// <param name="operationName">Name of the measured operation.</param>
    internal static long StopAndPrintElapsed(string operationName)
    {
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        stopwatch.Reset();
        Console.WriteLine(operationName + " takes " + elapsedMilliseconds + "ms");
        return elapsedMilliseconds;
    }
}
