using Algorithms.Interfaces;

namespace Algorithms.Lab2.IndividualTasks;

/// <summary>
///     Наивное рекурсивное вычисление чисел Фибоначчи:
///     F(0) = 0, F(1) = 1, F(n) = F(n − 1) + F(n − 2).
///     Промежуточные значения не кэшируются, поэтому число вызовов растёт экспоненциально.
/// </summary>
public sealed class NaiveFibonacci : IAlgorithm<int>
{
    /// <summary>
    ///     Ограничение связано с экспоненциальным временем: при n = 35 выполняется
    ///     около 30 млн рекурсивных вызовов, дальнейший рост быстро становится заметным.
    /// </summary>
    public const int MaxSupportedN = 35;

    public string Id => "naive-fibonacci";

    public string Name => "Наивное вычисление чисел Фибоначчи (Чикишев)";

    public int MaxN => MaxSupportedN;

    public string Description =>
        "Рекурсивно вычисляет F(n) по определению, без мемоизации и динамического программирования. " +
        "Используется как учебный пример экспоненциальной сложности.";

    /// <summary>Вход алгоритма — само число n.</summary>
    public int Generate(int n) => n;

    public void Execute(int n, int step)
    {
        // step для этого алгоритма не нужен: размер входа задаётся одним значением n.
        var value = Compute(n);
        GC.KeepAlive(value);
    }

    /// <summary>Возвращает n-е число Фибоначчи, вычисляя каждую ветвь рекурсии заново.</summary>
    public static long Compute(int n)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n));
        return n <= 1 ? n : Compute(n - 1) + Compute(n - 2);
    }
}
