using System;
using System.Linq;
using System.Threading.Tasks;
using Algorithms.Lab2.IndividualTasks;
using AlgosLabs.Services;
using AlgosLabs.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgosLabs.ViewModels;

/// <summary>
///     Состояние раздела индивидуального задания второй лабораторной работы:
///     вычисление F(N) наивной рекурсией и замер времени с построением графика.
/// </summary>
public partial class FibonacciViewModel : ObservableObject
{
    public const int MaxSupportedN = NaiveFibonacci.MaxSupportedN;

    [ObservableProperty]
    private int _n = 10;

    [ObservableProperty]
    private int _maxBenchmarkN = MaxSupportedN;

    [ObservableProperty]
    private string _resultText = "Результат появится здесь.";

    [ObservableProperty]
    private string _benchmarkStatus =
        "Нажмите «Построить график», чтобы измерить время вычисления от N.";

    [ObservableProperty]
    private bool _isBusy;

    /// <summary>Подпись допустимого диапазона для вычисления одного значения.</summary>
    public string NRangeText => $"от 0 до {MaxSupportedN}";

    /// <summary>Подпись допустимого диапазона для замера.</summary>
    public string MaxNRangeText => $"от 1 до {MaxSupportedN}";

    partial void OnNChanged(int value)
    {
        if (value < 0)
        {
            N = 0;
            return;
        }

        if (value > MaxSupportedN) N = MaxSupportedN;
    }

    partial void OnMaxBenchmarkNChanged(int value)
    {
        if (value < 1)
        {
            MaxBenchmarkN = 1;
            return;
        }

        if (value > MaxSupportedN) MaxBenchmarkN = MaxSupportedN;
    }

    /// <summary>Вычисляет F(N) и показывает значение в разделе.</summary>
    [RelayCommand]
    private async Task ComputeFibonacciAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var n = N;
            var value = await Task.Run(() => NaiveFibonacci.Compute(n));
            ResultText = $"F({n}) = {value:N0}";
        }
        catch (Exception ex)
        {
            ResultText = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Измеряет время наивного вычисления на отрезке [1, MaxBenchmarkN] и открывает график.</summary>
    [RelayCommand]
    private async Task RunFibonacciBenchmarkAsync()
    {
        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var maxN = MaxBenchmarkN;
            var points = await Task.Run(() => FibonacciBenchmarkService.Measure(maxN, 1));

            var chartData = new ChartDataModel();
            chartData.SeriesList.Add(new SeriesData(
                "Наивное вычисление Фибоначчи",
                points));

            var chartWindow = new ChartWindow
            {
                DataContext = new ChartWindowViewModel(
                    chartData,
                    "Время наивного вычисления чисел Фибоначчи",
                    "Номер числа Фибоначчи (N)",
                    "Время выполнения (мс)")
            };
            chartWindow.Show();

            var last = points.LastOrDefault();
            BenchmarkStatus = last == null
                ? "Нет данных для построения графика."
                : $"Измерено N = 1…{last.N}; при N = {last.N} — {last.AverageTimeMs:0.##} мс.";
        }
        catch (Exception ex)
        {
            BenchmarkStatus = $"Ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
