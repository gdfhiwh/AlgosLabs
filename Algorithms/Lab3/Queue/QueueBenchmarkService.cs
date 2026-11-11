using System.Diagnostics;
using System.IO;
using System.Text;
using Algorithms.Services;

namespace Algorithms.Lab3.Queue;

/// <summary>Какая реализация очереди используется.</summary>
public enum QueueKind
{
    List,
    Standard
}

/// <summary>
///     Результаты сравнения двух наборов одинаковой длины, но разного состава:
///     «глубокий» (много удалений при большой очереди) и «мелкий» (мало удалений).
/// </summary>
public sealed record QueueCompositionComparison(
    int Length,
    double ListDeepMs,
    double StandardDeepMs,
    double ListShallowMs,
    double StandardShallowMs);

/// <summary>
///     Замеры времени чтения файла и выполнения операций очереди
///     для обеих реализаций.
/// </summary>
public static class QueueBenchmarkService
{
    private const int DefaultRuns = 5;
    private const int ComparisonRuns = 3;

    /// <summary>Создаёт очередь выбранной реализации.</summary>
    public static IQueue<string> Create(QueueKind kind) => kind switch
    {
        QueueKind.List => new ListQueue<string>(),
        _ => new StandardQueue<string>()
    };

    /// <summary>Подпись реализации для интерфейса.</summary>
    public static string DisplayName(QueueKind kind) =>
        kind == QueueKind.List ? "Список (List<T>)" : "Стандартная Queue<T>";

    /// <summary>
    ///     Замеряет чтение файла и выполнение для нескольких длин набора.
    ///     Наборы устроены так, что глубина очереди растёт с N: списковая реализация
    ///     проявляет квадратичный рост, стандартная остаётся линейной.
    /// </summary>
    public static List<BenchmarkResultPoint> MeasureGrowing(QueueKind kind, int startN, int maxN, int stepN)
    {
        Normalize(ref startN, ref maxN, ref stepN);

        var points = new List<BenchmarkResultPoint>();
        var path = Path.Combine(Path.GetTempPath(), $"algoslabs_queue_{Guid.NewGuid():N}.txt");

        try
        {
            for (var n = startN; n <= maxN; n += stepN)
            {
                File.WriteAllText(path, GenerateGrowing(n, n), Encoding.UTF8);

                var samples = new double[DefaultRuns];
                for (var run = 0; run < samples.Length; run++)
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    var stopwatch = Stopwatch.StartNew();
                    var text = File.ReadAllText(path);
                    QueueCommandProcessor.Execute(Create(kind), text, collectLog: false);
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

    /// <summary>Замеряет чтение и выполнение конкретного файла.</summary>
    public static (double AverageMs, int OperationCount) MeasureFile(
        QueueKind kind,
        string path,
        int runs = DefaultRuns)
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
            operations = QueueCommandProcessor.Execute(Create(kind), text, collectLog: false).Executed;
            stopwatch.Stop();

            samples[run] = stopwatch.Elapsed.TotalMilliseconds;
        }

        return (Average(samples), operations);
    }

    /// <summary>Замеряет выполнение набора, записав его во временный файл.</summary>
    public static (double AverageMs, int OperationCount) MeasureText(
        QueueKind kind,
        string text,
        int runs = DefaultRuns)
    {
        var path = Path.Combine(Path.GetTempPath(), $"algoslabs_queue_{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, text, Encoding.UTF8);

        try
        {
            return MeasureFile(kind, path, runs);
        }
        finally
        {
            TryDelete(path);
        }
    }

    /// <summary>
    ///     Сравнивает наборы одинаковой длины с разным составом операций
    ///     для обеих реализаций.
    /// </summary>
    public static QueueCompositionComparison CompareCompositions(int length, int runs = ComparisonRuns)
    {
        length = Math.Max(10, length);

        var deep = GenerateDeep(length, 12345);
        var shallow = GenerateShallow(length, 54321);

        var listDeep = MeasureText(QueueKind.List, deep, runs).AverageMs;
        var standardDeep = MeasureText(QueueKind.Standard, deep, runs).AverageMs;
        var listShallow = MeasureText(QueueKind.List, shallow, runs).AverageMs;
        var standardShallow = MeasureText(QueueKind.Standard, shallow, runs).AverageMs;

        return new QueueCompositionComparison(
            length, listDeep, standardDeep, listShallow, standardShallow);
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
            // Временный файл не критичен.
        }
    }

    /// <summary>
    ///     Набор для замера «разной длины»: сначала очередь заполняется до N/4,
    ///     затем удаления чередуются со вставками, поэтому глубина остаётся большой.
    /// </summary>
    private static string GenerateGrowing(int tokenCount, int seed)
    {
        var random = new Random(seed);
        var builder = new StringBuilder(tokenCount * 6);
        var depth = 0;
        var fill = Math.Max(1, tokenCount / 4);

        for (var i = 0; i < tokenCount; i++)
        {
            if (i > 0) builder.Append(' ');

            var enqueue = depth == 0 || i < fill || i % 2 != 0;

            if (enqueue)
            {
                builder.Append("1,").Append(random.Next(0, 1000));
                depth++;
                continue;
            }

            var roll = random.Next(100);
            if (roll < 80)
            {
                builder.Append('2');
                depth--;
            }
            else if (roll < 92)
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
    ///     «Глубокий» набор: очередь наполняется до N/2, затем удаления чередуются
    ///     со вставками. Для списка каждое удаление сдвигает ~N/2 элементов.
    /// </summary>
    private static string GenerateDeep(int tokenCount, int seed)
    {
        var random = new Random(seed);
        var builder = new StringBuilder(tokenCount * 6);
        var depth = 0;
        var fill = Math.Max(1, tokenCount / 2);

        for (var i = 0; i < tokenCount; i++)
        {
            if (i > 0) builder.Append(' ');

            var dequeue = i >= fill && (i - fill) % 2 == 0 && depth > 0;

            if (dequeue)
            {
                builder.Append('2');
                depth--;
            }
            else
            {
                builder.Append("1,").Append(random.Next(0, 1000));
                depth++;
            }
        }

        return builder.ToString();
    }

    /// <summary>
    ///     «Мелкий» набор: длина та же, но вставки и удаления чередуются,
    ///     поэтому очередь почти всегда пуста и удаление дёшево для списка.
    /// </summary>
    private static string GenerateShallow(int tokenCount, int seed)
    {
        var random = new Random(seed);
        var builder = new StringBuilder(tokenCount * 6);
        var depth = 0;

        for (var i = 0; i < tokenCount; i++)
        {
            if (i > 0) builder.Append(' ');

            switch (i % 4)
            {
                case 0:
                    builder.Append("1,").Append(random.Next(0, 1000));
                    depth++;
                    break;

                case 1:
                    if (depth > 0)
                    {
                        builder.Append('2');
                        depth--;
                    }
                    else
                    {
                        builder.Append("1,").Append(random.Next(0, 1000));
                        depth++;
                    }

                    break;

                case 2:
                    builder.Append(depth > 0 ? '3' : '4');
                    break;

                default:
                    builder.Append('5');
                    break;
            }
        }

        return builder.ToString();
    }
}
