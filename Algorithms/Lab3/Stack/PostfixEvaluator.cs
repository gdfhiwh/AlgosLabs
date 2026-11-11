using System.Globalization;

namespace Algorithms.Lab3.Stack;

/// <summary>Результат вычисления выражения в постфиксной записи.</summary>
public sealed record PostfixEvaluationResult(
    bool Success,
    double Value,
    string Error,
    IReadOnlyList<string> Steps,
    int TokenCount);

/// <summary>
///     Вычисление выражения в постфиксной записи с использованием собственного стека.
///     Поддерживаются операции: +, -, *, : (или /), ^, ln, cos, sin, sqrt.
///     Время работы — O(n), дополнительная память — O(n), где n — число токенов.
/// </summary>
public static class PostfixEvaluator
{
    public static PostfixEvaluationResult Evaluate(string expression)
    {
        var stack = new LinkedStack<double>();
        var steps = new List<string>();
        var tokens = expression.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var count = 0;

        PostfixEvaluationResult Fail(string error) =>
            new(false, double.NaN, error, steps, count);

        foreach (var raw in tokens)
        {
            var token = raw.Trim();
            if (token.Length == 0) continue;

            count++;

            // Сначала пробуем число: так отрицательные значения вроде «-5» не путаются с минусом.
            if (TryParseNumber(token, out var number))
            {
                stack.Push(number);
                steps.Add($"«{token}»: Push({Format(number)})");
                continue;
            }

            var operation = token.ToLowerInvariant();

            if (operation is "+" or "-" or "*" or ":" or "/" or "^")
            {
                if (stack.Count < 2)
                    return Fail($"для операции «{token}» нужно два операнда");

                var right = stack.Pop();
                var left = stack.Pop();

                double value;
                switch (operation)
                {
                    case "+":
                        value = left + right;
                        break;
                    case "-":
                        value = left - right;
                        break;
                    case "*":
                        value = left * right;
                        break;
                    case ":":
                    case "/":
                        if (right == 0) return Fail("деление на ноль");
                        value = left / right;
                        break;
                    default:
                        value = Math.Pow(left, right);
                        break;
                }

                stack.Push(value);
                steps.Add($"«{token}»: {Format(left)} {operation} {Format(right)} = {Format(value)}");
                continue;
            }

            if (operation is "ln" or "cos" or "sin" or "sqrt")
            {
                if (stack.IsEmpty)
                    return Fail($"для операции «{token}» нужен один операнд");

                var operand = stack.Pop();

                double value;
                switch (operation)
                {
                    case "ln":
                        if (operand <= 0) return Fail("ln определён только для положительных чисел");
                        value = Math.Log(operand);
                        break;
                    case "cos":
                        value = Math.Cos(operand);
                        break;
                    case "sin":
                        value = Math.Sin(operand);
                        break;
                    default:
                        if (operand < 0) return Fail("sqrt определён только для неотрицательных чисел");
                        value = Math.Sqrt(operand);
                        break;
                }

                stack.Push(value);
                steps.Add($"«{token}»: {operation}({Format(operand)}) = {Format(value)}");
                continue;
            }

            // В задании в перечне операций встречается «)» — в постфиксной записи скобки не нужны,
            // поэтому такой токен просто пропускаем, не считая ошибкой.
            if (operation == ")")
            {
                steps.Add("«)»: пропущено (в постфиксной записи скобки не нужны)");
                continue;
            }

            return Fail($"неизвестный токен «{token}»");
        }

        if (stack.IsEmpty)
            return Fail("выражение пустое");

        if (stack.Count > 1)
            return Fail($"в стеке осталось {stack.Count} значений — выражение некорректно");

        return new PostfixEvaluationResult(true, stack.Top(), string.Empty, steps, count);
    }

    /// <summary>Разбирает число с точкой или запятой в качестве десятичного разделителя.</summary>
    private static bool TryParseNumber(string token, out double value)
    {
        value = 0;
        var normalized = token.Replace(',', '.');
        return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static string Format(double value) =>
        value.ToString("G8", CultureInfo.InvariantCulture);
}
