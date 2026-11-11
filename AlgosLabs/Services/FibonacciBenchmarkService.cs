using System;
using System.Collections.Generic;
using System.Diagnostics;
using Algorithms.Lab2.IndividualTasks;
using Algorithms.Services;

namespace AlgosLabs.Services;

/// <summary>
///     Замеряет только рекурсивное вычисление чисел Фибоначчи, без кэша и работы интерфейса.
///     Число вызовов растёт экспоненциально, поэтому перед каждой точкой убирается мусор,
///     а результат усредняется по нескольким прогонам.
/// </summary>
public static class FibonacciBenchmarkService
{
    /// <summary>Число прогонов на одну точку: сглаживает шум планировщика.</summary>
    private const int Runs = 3;

    public static List<BenchmarkResultPoint> Measure(int maxN, int step)
    {
        var limit = Math.Clamp(maxN, 1, NaiveFibonacci.MaxSupportedN);
        var nStep = Math.Max(1, step);

        var result = new List<BenchmarkResultPoint>();

        // Прогрев JIT: первый вызов заметно медленнее из-за компиляции.
        NaiveFibonacci.Compute(Math.Min(10, limit));

        var stopwatch = new Stopwatch();

        for (var n = 1; n <= limit; n += nStep)
        {
            // Сборка мусора вынесена за пределы секундомера.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            stopwatch.Restart();
            for (var run = 0; run < Runs; run++) NaiveFibonacci.Compute(n);
            stopwatch.Stop();

            result.Add(new BenchmarkResultPoint(n, stopwatch.Elapsed.TotalMilliseconds / Runs));
        }

        return result;
    }
}
