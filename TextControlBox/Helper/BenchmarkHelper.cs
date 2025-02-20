using System;
using System.Diagnostics;

namespace TextControlBoxNS.Helper;

internal class BenchmarkHelper
{
    public static void Run(Action action, string name = "")
    {
        if (action == null)
            throw new ArgumentNullException(nameof(action));

        // Force a garbage collection to get a clean memory baseline.
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        //Get initial memory usage.
        long initialMemory = GC.GetTotalMemory(true);

        //Measure execution time.
        var stopwatch = Stopwatch.StartNew();
        action();
        stopwatch.Stop();

        //Get final memory usage.
        long finalMemory = GC.GetTotalMemory(false);

        //Calculate results.
        long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        long elapsedTicks = stopwatch.ElapsedTicks;
        long memoryUsed = finalMemory - initialMemory;

        //Log results.
        Debug.WriteLine("Benchmark: " + name);
        Debug.WriteLine($"Execution Time: {elapsedMilliseconds} ms | {elapsedTicks} Ticks");
        Debug.WriteLine($"Memory Used: {memoryUsed / 1024.0:F2} KB");
    }
}
