using System;
using System.Linq;
using System.Threading.Tasks;
using AlgosLabs.Services;
using AlgosLabs.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgosLabs.ViewModels;

public partial class SylvesterViewModel : ObservableObject
{
    public const int MaxSupportedN = 40;

    [ObservableProperty] private int _n = 5;
    [ObservableProperty] private int _maxBenchmarkN = 10;
    [ObservableProperty] private string _resultText = "Результат появится здесь.";
    [ObservableProperty]
    private string _benchmarkStatus =
        "Нажмите «Построить график», чтобы измерить время вычисления от N.";
    [ObservableProperty] private bool _isBusy;

    public string NRangeText => $"от 0 до {MaxSupportedN}";
    public string MaxNRangeText => $"от 1 до {MaxSupportedN}";

    partial void OnNChanged(int value)
    {
        if (value < 0) N = 0;
        else if (value > MaxSupportedN) N = MaxSupportedN;
    }

    partial void OnMaxBenchmarkNChanged(int value)
    {
        if (value < 1) MaxBenchmarkN = 1;
        else if (value > MaxSupportedN) MaxBenchmarkN = MaxSupportedN;
    }

    [RelayCommand]
    private async Task ComputeSylvesterAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var n = N;
            var value = await Task.Run(() => SylvesterBenchmarkService.Compute(n));
            var str = value.ToString();
            ResultText = str.Length > 200
                ? $"a({n}) = {str[..100]}... [ещё {str.Length - 200} цифр] ...{str[^50..]}\n(всего цифр: {str.Length})"
                : $"a({n}) = {str}";
        }
        catch (Exception ex) { ResultText = $"Ошибка: {ex.Message}"; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task RunSylvesterBenchmarkAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            var points = await Task.Run(() => SylvesterBenchmarkService.Measure(MaxBenchmarkN, 1));
            var chartData = new ChartDataModel();
            chartData.SeriesList.Add(new SeriesData("Последовательность Сильвестра", points));
            new ChartWindow
            {
                DataContext = new ChartWindowViewModel(
                    chartData,
                    "Время вычисления последовательности Сильвестра",
                    "Номер члена (N)",
                    "Время (мс)")
            }.Show();

            var last = points.LastOrDefault();
            BenchmarkStatus = last == null
                ? "Нет данных."
                : $"Измерено N = 1…{last.N}; при N = {last.N} — {last.AverageTimeMs:0.##} мс.";
        }
        catch (Exception ex) { BenchmarkStatus = $"Ошибка: {ex.Message}"; }
        finally { IsBusy = false; }
    }
}