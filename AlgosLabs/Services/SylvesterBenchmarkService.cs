using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using Algorithms.Lab2;          // namespace оригинального файла!
using Algorithms.Services;      // BenchmarkResultPoint

namespace AlgosLabs.Services;

public static class SylvesterBenchmarkService
{
    private const int Runs = 3;

    // Значение совпадает с instance-свойством MaxN оригинального алгоритма.
    private const int MaxSupportedN = 40;

    /// <summary>
    /// Итеративное вычисление n-го члена (дублирует private SylvesterCore оригинала),
    /// чтобы ViewModel мог показать результат, не меняя основной файл.
    /// </summary>
    public static BigInteger Compute(int n)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
        BigInteger current = 2;
        for (var i = 1; i <= n; i++)
            current = current * (current - 1) + 1;
        return current;
    }

    public static List<BenchmarkResultPoint> Measure(int maxN, int step)
    {
        var limit = Math.Clamp(maxN, 1, MaxSupportedN);
        var nStep = Math.Max(1, step);
        var result = new List<BenchmarkResultPoint>();

        // Прогрев JIT.
        Compute(Math.Min(5, limit));

        var stopwatch = new Stopwatch();
        for (var n = 1; n <= limit; n += nStep)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch.Restart();
            for (var run = 0; run < Runs; run++)
                Compute(n);
            stopwatch.Stop();

            result.Add(new BenchmarkResultPoint(n, stopwatch.Elapsed.TotalMilliseconds / Runs));
        }
        return result;
    }
}