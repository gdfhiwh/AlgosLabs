using System.Text;

namespace Algorithms.Lab3.Stack;

/// <summary>Одна выполненная команда стека и её результат.</summary>
public sealed record StackOperationLog(int Index, string Command, string Result);

/// <summary>Итог выполнения набора команд из файла.</summary>
public sealed class StackCommandResult
{
    public List<StackOperationLog> Log { get; } = new();
    public string FinalStack { get; set; } = "[]";
    public int Executed { get; set; }
    public int Errors { get; set; }
}

/// <summary>
///     Разбирает и выполняет команды работы со стеком:
///     1,значение — Push, 2 — Pop, 3 — Top, 4 — isEmpty, 5 — Print.
/// </summary>
public static class StackCommandProcessor
{
    /// <param name="text">Содержимое файла с командами.</param>
    /// <param name="collectLog">
    ///     Собирать ли журнал операций. В замерах журнал не нужен — построение
    ///     строк исказило бы время выполнения самих операций стека.
    /// </param>
    public static StackCommandResult Execute(string text, bool collectLog = true)
    {
        var result = new StackCommandResult();
        var stack = new LinkedStack<string>();
        var tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        var index = 0;
        foreach (var token in tokens)
        {
            index++;
            string? outcome = null;

            // Push записывается как «1,значение»: значение может быть числом или словом.
            if (token.StartsWith("1,", StringComparison.Ordinal))
            {
                var value = token[2..];

                if (value.Length == 0)
                {
                    result.Errors++;
                    if (collectLog) outcome = "Ошибка: не указано значение для Push";
                }
                else
                {
                    stack.Push(value);
                    if (collectLog) outcome = $"Push({value}) → стек: {stack.Print()}";
                }
            }
            else
            {
                switch (token)
                {
                    case "1":
                        result.Errors++;
                        if (collectLog) outcome = "Ошибка: для Push нужно значение (1,элемент)";
                        break;

                    case "2":
                        try
                        {
                            var value = stack.Pop();
                            if (collectLog) outcome = $"Pop() → {value}";
                        }
                        catch (InvalidOperationException)
                        {
                            result.Errors++;
                            if (collectLog) outcome = "Ошибка: стек пуст";
                        }

                        break;

                    case "3":
                        try
                        {
                            var value = stack.Top();
                            if (collectLog) outcome = $"Top() → {value}";
                        }
                        catch (InvalidOperationException)
                        {
                            result.Errors++;
                            if (collectLog) outcome = "Ошибка: стек пуст";
                        }

                        break;

                    case "4":
                        if (collectLog)
                            outcome = stack.IsEmpty ? "isEmpty() → true" : "isEmpty() → false";
                        break;

                    case "5":
                        // Print — реальная операция: даже в замере она должна выполняться.
                        var printed = stack.Print();
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
                result.Log.Add(new StackOperationLog(index, token, outcome));
        }

        if (collectLog) result.FinalStack = stack.Print();
        return result;
    }

    /// <summary>Форматирует журнал операций для показа на экране.</summary>
    public static string FormatLog(StackCommandResult result)
    {
        var builder = new StringBuilder();

        foreach (var entry in result.Log)
            builder.Append(entry.Index.ToString().PadLeft(3))
                .Append(". ")
                .Append(entry.Command.PadRight(8))
                .Append(" → ")
                .AppendLine(entry.Result);

        builder.AppendLine();
        builder.AppendLine($"Выполнено команд: {result.Executed}, ошибок: {result.Errors}");
        builder.AppendLine($"Итоговый стек (сверху вниз): {result.FinalStack}");

        return builder.ToString();
    }
}
