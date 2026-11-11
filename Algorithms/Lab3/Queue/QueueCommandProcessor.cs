using System.Text;

namespace Algorithms.Lab3.Queue;

/// <summary>Одна выполненная команда очереди и её результат.</summary>
public sealed record QueueOperationLog(int Index, string Command, string Result);

/// <summary>Итог выполнения набора команд очереди.</summary>
public sealed class QueueCommandResult
{
    public List<QueueOperationLog> Log { get; } = new();
    public string FinalQueue { get; set; } = "[]";
    public int Executed { get; set; }
    public int Errors { get; set; }
}

/// <summary>
///     Разбирает и выполняет команды очереди:
///     1,значение — вставка, 2 — удаление, 3 — просмотр начала,
///     4 — проверка на пустоту, 5 — печать.
/// </summary>
public static class QueueCommandProcessor
{
    /// <param name="queue">Конкретная реализация очереди (список или стандартная).</param>
    /// <param name="text">Содержимое файла с командами.</param>
    /// <param name="collectLog">
    ///     Собирать ли журнал. В замерах журнал не нужен — он исказил бы время.
    /// </param>
    public static QueueCommandResult Execute(IQueue<string> queue, string text, bool collectLog = true)
    {
        var result = new QueueCommandResult();
        var tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        var index = 0;
        foreach (var token in tokens)
        {
            index++;
            string? outcome = null;

            // Вставка записывается как «1,значение»: значение может быть числом или словом.
            if (token.StartsWith("1,", StringComparison.Ordinal))
            {
                var value = token[2..];

                if (value.Length == 0)
                {
                    result.Errors++;
                    if (collectLog) outcome = "Ошибка: не указано значение для вставки";
                }
                else
                {
                    queue.Enqueue(value);
                    if (collectLog) outcome = $"Enqueue({value}) → очередь: {queue.Print()}";
                }
            }
            else
            {
                switch (token)
                {
                    case "1":
                        result.Errors++;
                        if (collectLog) outcome = "Ошибка: для вставки нужно значение (1,элемент)";
                        break;

                    case "2":
                        try
                        {
                            var value = queue.Dequeue();
                            if (collectLog) outcome = $"Dequeue() → {value}";
                        }
                        catch (InvalidOperationException)
                        {
                            result.Errors++;
                            if (collectLog) outcome = "Ошибка: очередь пуста";
                        }

                        break;

                    case "3":
                        try
                        {
                            var value = queue.Peek();
                            if (collectLog) outcome = $"Peek() → {value}";
                        }
                        catch (InvalidOperationException)
                        {
                            result.Errors++;
                            if (collectLog) outcome = "Ошибка: очередь пуста";
                        }

                        break;

                    case "4":
                        if (collectLog)
                            outcome = queue.IsEmpty ? "isEmpty() → true" : "isEmpty() → false";
                        break;

                    case "5":
                        // Print — реальная операция: даже в замере она должна выполняться.
                        var printed = queue.Print();
                        if (collectLog) outcome = $"Print() → {printed}";
                        break;

                    default:
                        result.Errors++;
                        if (collectLog) outcome = $"Ошибка: неизвестная команда «{token}»";
                        break;
                }
            }

            result.Executed++;

            if (collectLog && outcome is not null)
                result.Log.Add(new QueueOperationLog(index, token, outcome));
        }

        if (collectLog) result.FinalQueue = queue.Print();
        return result;
    }

    /// <summary>Форматирует журнал операций для показа на экране.</summary>
    public static string FormatLog(QueueCommandResult result, string implementationName)
    {
        var builder = new StringBuilder();
        builder.AppendLine($"Реализация: {implementationName}");
        builder.AppendLine();

        foreach (var entry in result.Log)
            builder.Append(entry.Index.ToString().PadLeft(3))
                .Append(". ")
                .Append(entry.Command.PadRight(8))
                .Append(" → ")
                .AppendLine(entry.Result);

        builder.AppendLine();
        builder.AppendLine($"Выполнено команд: {result.Executed}, ошибок: {result.Errors}");
        builder.AppendLine($"Итоговая очередь (от начала к концу): {result.FinalQueue}");

        return builder.ToString();
    }
}
