using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Algorithms.Lab3.Queue;
using AlgosLabs.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgosLabs.ViewModels;

/// <summary>
///     Раздел «Очередь» третьей лабы: операции из файла, замеры двух реализаций
///     и сравнение наборов одинаковой длины с разным составом.
/// </summary>
public partial class QueueViewModel : ObservableObject
{
    private const string SampleCommands = "3 4 1,56 1,7 1,cat 2 5 4";

    public QueueViewModel()
    {
        _inputFilePath = Path.Combine(AppContext.BaseDirectory, "input.txt");
    }

    [ObservableProperty] private string _inputFilePath;
    [ObservableProperty] private string _queueLog = "Файл ещё не выполнялся.";
    [ObservableProperty] private string _queueFinalState = "Очередь пуста.";
    [ObservableProperty] private string _compositionReport = "Сравнение составов ещё не выполнялось.";
    [ObservableProperty] private bool _isQueueBusy;

    /// <summary>0 — список, 1 — стандартная Queue&lt;T&gt;.</summary>
    [ObservableProperty] private int _selectedImplementationIndex;

    public IReadOnlyList<string> ImplementationNames { get; } = new[]
    {
        "Список (List<T>)",
        "Стандартная Queue<T>"
    };

    // Параметры замера «разной длины».

    [ObservableProperty] private int _benchmarkStartN = 1000;
    [ObservableProperty] private int _benchmarkMaxN = 20000;
    [ObservableProperty] private int _benchmarkStepN = 2000;

    /// <summary>Длина каждого из двух наборов при сравнении состава.</summary>
    [ObservableProperty] private int _compositionLength = 20000;

    private QueueKind SelectedKind =>
        SelectedImplementationIndex == 1 ? QueueKind.Standard : QueueKind.List;

    /// <summary>Создаёт input.txt с примером из задания.</summary>
    [RelayCommand]
    private void CreateSampleInputFile()
    {
        try
        {
            File.WriteAllText(InputFilePath, SampleCommands, Encoding.UTF8);
            QueueLog = $"Создан файл:\n{InputFilePath}\n\nСодержимое:\n{SampleCommands}";
            QueueFinalState = "Очередь пуста.";
        }
        catch (Exception ex)
        {
            QueueLog = $"Не удалось создать файл: {ex.Message}";
        }
    }

    /// <summary>Читает input.txt, выполняет команды выбранной реализацией и показывает результат.</summary>
    [RelayCommand]
    private async Task RunQueueOperationsAsync()
    {
        if (IsQueueBusy) return;
        IsQueueBusy = true;

        try
        {
            var kind = SelectedKind;
            var name = QueueBenchmarkService.DisplayName(kind);
            var path = InputFilePath;

            var text = await Task.Run(() => File.ReadAllText(path));
            var result = await Task.Run(() =>
                QueueCommandProcessor.Execute(QueueBenchmarkService.Create(kind), text));

            QueueLog = QueueCommandProcessor.FormatLog(result, name);
            QueueFinalState = $"Очередь (от начала к концу): {result.FinalQueue}";
        }
        catch (Exception ex)
        {
            QueueLog = $"Не удалось прочитать файл «{InputFilePath}»:\n{ex.Message}";
            QueueFinalState = "Очередь пуста.";
        }
        finally
        {
            IsQueueBusy = false;
        }
    }

    /// <summary>Замеряет чтение и выполнение текущего файла выбранной реализацией.</summary>
    [RelayCommand]
    private async Task MeasureCurrentInputFileAsync()
    {
        if (IsQueueBusy) return;
        IsQueueBusy = true;

        try
        {
            var kind = SelectedKind;
            var path = InputFilePath;
            var (averageMs, operations) = await Task.Run(() => QueueBenchmarkService.MeasureFile(kind, path));

            var builder = new StringBuilder();
            builder.AppendLine($"Реализация: {QueueBenchmarkService.DisplayName(kind)}");
            builder.AppendLine($"Файл: {path}");
            builder.AppendLine($"Команд выполнено: {operations}");
            builder.AppendLine($"Среднее время чтения и выполнения: {averageMs:F4} мс (5 прогонов)");

            QueueLog = builder.ToString();
            QueueFinalState = "Замер выполнен.";
        }
        catch (Exception ex)
        {
            QueueLog = $"Не удалось выполнить замер файла «{InputFilePath}»:\n{ex.Message}";
        }
        finally
        {
            IsQueueBusy = false;
        }
    }

    /// <summary>Строит график для обеих реализаций при разных длинах набора.</summary>
    [RelayCommand]
    private async Task RunQueueBenchmarkAsync()
    {
        if (IsQueueBusy) return;
        IsQueueBusy = true;

        try
        {
            var startN = BenchmarkStartN;
            var maxN = BenchmarkMaxN;
            var stepN = BenchmarkStepN;

            var (listPoints, standardPoints) = await Task.Run(() => (
                QueueBenchmarkService.MeasureGrowing(QueueKind.List, startN, maxN, stepN),
                QueueBenchmarkService.MeasureGrowing(QueueKind.Standard, startN, maxN, stepN)));

            var chartData = new ChartDataModel();
            chartData.SeriesList.Add(new SeriesData(
                QueueBenchmarkService.DisplayName(QueueKind.List), listPoints));
            chartData.SeriesList.Add(new SeriesData(
                QueueBenchmarkService.DisplayName(QueueKind.Standard), standardPoints));

            new ChartWindow
            {
                DataContext = new ChartWindowViewModel(
                    chartData,
                    "Время считывания и выполнения операций очереди",
                    "Количество операций (N)",
                    "Время (мс)")
            }.Show();
        }
        catch (Exception ex)
        {
            QueueLog = $"Ошибка замера очереди: {ex.Message}";
        }
        finally
        {
            IsQueueBusy = false;
        }
    }

    /// <summary>Сравнивает два набора одинаковой длины с разным составом операций.</summary>
    [RelayCommand]
    private async Task RunCompositionComparisonAsync()
    {
        if (IsQueueBusy) return;
        IsQueueBusy = true;

        try
        {
            var length = CompositionLength;
            var comparison = await Task.Run(() => QueueBenchmarkService.CompareCompositions(length));
            CompositionReport = FormatComparison(comparison);
        }
        catch (Exception ex)
        {
            CompositionReport = $"Ошибка сравнения составов: {ex.Message}";
        }
        finally
        {
            IsQueueBusy = false;
        }
    }

    private static string FormatComparison(QueueCompositionComparison comparison)
    {
        static string Ms(double value) => value.ToString("F4", CultureInfo.InvariantCulture);
        static double Ratio(double value, double baseline) => value / Math.Max(baseline, 1e-6);

        var builder = new StringBuilder();
        builder.AppendLine($"Длина каждого набора: {comparison.Length} команд");
        builder.AppendLine();

        builder.AppendLine("Глубокий набор (много удалений при большой очереди):");
        builder.AppendLine($"  Список (List<T>):     {Ms(comparison.ListDeepMs)} мс");
        builder.AppendLine($"  Стандартная Queue<T>: {Ms(comparison.StandardDeepMs)} мс");
        builder.AppendLine($"  List медленнее в {Ratio(comparison.ListDeepMs, comparison.StandardDeepMs):F1}×");
        builder.AppendLine();

        builder.AppendLine("Мелкий набор (мало удалений, очередь почти пуста):");
        builder.AppendLine($"  Список (List<T>):     {Ms(comparison.ListShallowMs)} мс");
        builder.AppendLine($"  Стандартная Queue<T>: {Ms(comparison.StandardShallowMs)} мс");
        builder.AppendLine($"  List медленнее в {Ratio(comparison.ListShallowMs, comparison.StandardShallowMs):F1}×");
        builder.AppendLine();

        builder.AppendLine("Расхождение между составами одинаковой длины:");
        builder.AppendLine(
            $"  List:  {Ratio(comparison.ListDeepMs, comparison.ListShallowMs):F1}× (глубокий / мелкий)");
        builder.AppendLine(
            $"  Queue: {Ratio(comparison.StandardDeepMs, comparison.StandardShallowMs):F1}× (глубокий / мелкий)");
        builder.AppendLine();
        builder.Append(
            "Вывод: при одинаковой длине набор с большой глубиной очереди делает удаление из списка дорогим " +
            "(сдвиг всех элементов), а Queue<T> остаётся быстрым.");

        return builder.ToString();
    }
}
