using System;
using System.Numerics;
using Algorithms.Interfaces;
using Algorithms.Lab1.PowerOperations;

namespace Algorithms.Lab2.IndividualTasks;

public class SylvesterSequence : IAlgorithm<int>, IIndividualAlgorithm
{
    public string Name => "Последовательность Сильвестра (Бурлев)";
    public string Id => "SylvesterSequence";

    // Ограничиваем MaxN = 15. 
    // Уже при n=15 результат содержит десятки тысяч цифр, 
    // что может привести к нехватке памяти или таймауту.
    public int MaxN => 40;

    /// <summary>
    /// Рекурсивное вычисление n-го члена последовательности Сильвестра.
    /// a(0) = 2
    /// a(n) = a(n-1) * (a(n-1) - 1) + 1
    /// </summary> 
    public void Execute(int n, int step)
    {
        // step для этого алгоритма не нужен: размер входа задаётся одним значением n.
        var value = SylvesterCore(n);
        GC.KeepAlive(value);
    }
    public static BigInteger SylvesterCore(int n)
    {
        // Базовый случай рекурсии
        if (n == 0)
        {
            return 2;
        }

        // Рекурсивный шаг
        BigInteger prev = SylvesterCore(n - 1);
        return prev * (prev - 1) + 1;
    }

    public int Generate(int n) => n;
}