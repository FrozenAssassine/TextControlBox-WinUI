using System;
using System;
using System.Diagnostics;

namespace TextControlBoxNS.Helper
{
    internal class BenchmarkHelper
    {
        /// <summary>
        /// Benchmarks the provided action and logs the time and memory usage.
        /// </summary>
        /// <param name="action">The action to benchmark.</param>
        public static void Run(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            // Force a garbage collection to get a clean memory baseline.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Get initial memory usage.
            long initialMemory = GC.GetTotalMemory(true);

            // Measure execution time.
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();

            // Get final memory usage.
            long finalMemory = GC.GetTotalMemory(false);

            // Calculate results.
            long elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            long memoryUsed = finalMemory - initialMemory;

            // Log results.
            Debug.WriteLine($"Execution Time: {elapsedMilliseconds} ms");
            Debug.WriteLine($"Memory Used: {memoryUsed / 1024.0:F2} KB");
        }
    }

}
