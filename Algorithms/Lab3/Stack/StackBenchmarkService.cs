using System.Diagnostics;
using System.IO;
using System.Text;
using Algorithms.Services;

namespace Algorithms.Lab3.Stack;

/// <summary>
///     Замеры времени: считывание файла с командами и выполнение операций стека,
///     а также вычисление постфиксных выражений.
/// </summary>
public static class StackBenchmarkService
{
    private const int DefaultRuns = 5;

    /// <summary>
    ///     Замеряет чтение файла и выполнение набора команд для нескольких размеров
    ///     N. Наборы генерируются детерминированно, но различны по составу.
    /// </summary>
    public static List<BenchmarkResultPoint> MeasureCommandExecution(int startN, int maxN, int stepN)
    {
        Normalize(ref startN, ref maxN, ref stepN);

        var points = new List<BenchmarkResultPoint>();
        var path = Path.Combine(Path.GetTempPath(), $"algoslabs_stack_{Guid.NewGuid():N}.txt");

        try
        {
            for (var n = startN; n <= maxN; n += stepN)
            {
                // Файл готовим заранее: в замер попадает именно чтение и выполнение.
                File.WriteAllText(path, GenerateCommands(n, n), Encoding.UTF8);

                var samples = new double[DefaultRuns];
                for (var run = 0; run < samples.Length; run++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    var stopwatch = Stopwatch.StartNew();
                    var text = File.ReadAllText(path);
                    StackCommandProcessor.Execute(text, collectLog: false);
                    stopwatch.Stop();

                    samples[run] = stopwatch.Elapsed.TotalMilliseconds;
                }

                points.Add(new BenchmarkResultPoint(n, Average(samples)));
            }
        }
        finally
        {
            TryDelete(path);
        }

        return points;
    }

    /// <summary>Замеряет чтение и выполнение конкретного файла с командами.</summary>
    public static (double AverageMs, int OperationCount) MeasureFile(string path, int runs = DefaultRuns)
    {
        var sampleCount = Math.Max(1, runs);
        var samples = new double[sampleCount];
        var operations = 0;

        for (var run = 0; run < sampleCount; run++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            var stopwatch = Stopwatch.StartNew();
            var text = File.ReadAllText(path);
            operations = StackCommandProcessor.Execute(text, collectLog: false).Executed;
            stopwatch.Stop();

            samples[run] = stopwatch.Elapsed.TotalMilliseconds;
        }

        return (Average(samples), operations);
    }

    /// <summary>
    ///     Замеряет вычисление постфиксной записи для выражений разной длины.
    ///     Используется для эмпирической оценки сложности алгоритма.
    /// </summary>
    public static List<BenchmarkResultPoint> MeasurePostfixEvaluation(int startN, int maxN, int stepN)
    {
        Normalize(ref startN, ref maxN, ref stepN);

        var points = new List<BenchmarkResultPoint>();

        for (var n = startN; n <= maxN; n += stepN)
        {
            var expression = GeneratePostfix(n, n);
            var samples = new double[DefaultRuns];

            for (var run = 0; run < samples.Length; run++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                var stopwatch = Stopwatch.StartNew();
                var result = PostfixEvaluator.Evaluate(expression);
                stopwatch.Stop();

                if (!result.Success)
                    throw new InvalidOperationException(
                        $"Сгенерированное выражение некорректно: {result.Error}");

                samples[run] = stopwatch.Elapsed.TotalMilliseconds;
            }

            points.Add(new BenchmarkResultPoint(n, Average(samples)));
        }

        return points;
    }

    private static void Normalize(ref int startN, ref int maxN, ref int stepN)
    {
        startN = Math.Max(1, startN);
        stepN = Math.Max(1, stepN);
        maxN = Math.Max(startN, maxN);
    }

    private static double Average(double[] samples)
    {
        var sum = 0.0;
        foreach (var sample in samples) sum += sample;
        return sum / samples.Length;
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // Временный файл не критичен для работы приложения.
        }
    }

    /// <summary>
    ///     Генерирует набор из <paramref name="tokenCount"/> команд. Pop/Top/isEmpty/Print
    ///     выбираются только при непустом стеке, чтобы замер не ломался на исключениях.
    /// </summary>
    private static string GenerateCommands(int tokenCount, int seed)
    {
        var random = new Random(seed);
        var builder = new StringBuilder(tokenCount * 6);
        var depth = 0;

        for (var i = 0; i < tokenCount; i++)
        {
            if (i > 0) builder.Append(' ');

            if (depth == 0)
            {
                // Пустой стек — Pop/Top невозможны, поэтому обязательно Push.
                builder.Append("1,").Append(random.Next(0, 1000));
                depth++;
                continue;
            }

            var roll = random.Next(100);
            if (roll < 60)
            {
                builder.Append("1,").Append(random.Next(0, 1000));
                depth++;
            }
            else if (roll < 70)
            {
                builder.Append('2');
                depth--;
            }
            else if (roll < 85)
            {
                builder.Append('3');
            }
            else
            {
                builder.Append('4');
            }
        }

        return builder.ToString();
    }

    /// <summary>
    ///     Генерирует корректное постфиксное выражение примерно из
    ///     <paramref name="tokenCount"/> токенов. Операции + и - сохраняют
    ///     значения конечными, а cos безопасен для любого числа.
    /// </summary>
    private static string GeneratePostfix(int tokenCount, int seed)
    {
        var random = new Random(seed);
        var operands = Math.Max(1, tokenCount / 2);
        var builder = new StringBuilder(tokenCount * 4);

        void Append(string token)
        {
            if (builder.Length > 0) builder.Append(' ');
            builder.Append(token);
        }

        void AppendOperand() => Append(random.Next(1, 10).ToString());

        AppendOperand();
        for (var i = 1; i < operands; i++)
        {
            AppendOperand();
            Append(random.Next(2) == 0 ? "+" : "-");
        }

        // Цепочка из k операндов и k-1 бинарных операций даёт 2k-1 токенов.
        // Недостающие до tokenCount токены добавляем унарными операциями.
        var produced = 2 * operands - 1;
        while (produced < tokenCount)
        {
            Append("cos");
            produced++;
        }

        return builder.ToString();
    }
}
