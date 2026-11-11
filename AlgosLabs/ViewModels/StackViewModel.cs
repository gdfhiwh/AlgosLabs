using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Algorithms.Lab3.Stack;
using AlgosLabs.Services;
using AlgosLabs.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgosLabs.ViewModels;

/// <summary>
///     Раздел «Стек» третьей лабы: операции из файла, замеры времени
///     и вычисление выражений в постфиксной записи.
/// </summary>
public partial class StackViewModel : ObservableObject
{
    private const string SampleCommands = "3 4 1,56 1,7 1,cat 2 5 4";
    private const string SamplePostfix = "2 3 ^ 16 sqrt + 4 2 / +";

    public StackViewModel()
    {
        _inputFilePath = Path.Combine(AppContext.BaseDirectory, "input.txt");
        _postfixFilePath = Path.Combine(AppContext.BaseDirectory, "postfix.txt");
    }

    // Пути к файлам с данными.

    [ObservableProperty] private string _inputFilePath;
    [ObservableProperty] private string _postfixFilePath;

    // Результаты выполнения.

    [ObservableProperty] private string _stackLog = "Файл ещё не выполнялся.";
    [ObservableProperty] private string _stackFinalState = "Стек пуст.";
    [ObservableProperty] private string _postfixLog = "Выражение ещё не вычислялось.";
    [ObservableProperty] private string _postfixResult = "—";
    [ObservableProperty] private bool _isStackBusy;

    [ObservableProperty]
    private string _postfixComplexity =
        "Теоретическая оценка: O(n) по времени и O(n) по памяти, где n — число токенов выражения.";

    // Параметры замеров (N — количество операций / токенов).

    [ObservableProperty] private int _stackBenchmarkStartN = 100;
    [ObservableProperty] private int _stackBenchmarkMaxN = 10000;
    [ObservableProperty] private int _stackBenchmarkStepN = 500;

    [ObservableProperty] private int _postfixBenchmarkStartN = 100;
    [ObservableProperty] private int _postfixBenchmarkMaxN = 20000;
    [ObservableProperty] private int _postfixBenchmarkStepN = 1000;

    /// <summary>Создаёт input.txt с примером из задания.</summary>
    [RelayCommand]
    private void CreateSampleInputFile()
    {
        try
        {
            File.WriteAllText(InputFilePath, SampleCommands, Encoding.UTF8);
            StackLog = $"Создан файл:\n{InputFilePath}\n\nСодержимое:\n{SampleCommands}";
            StackFinalState = "Стек пуст.";
        }
        catch (Exception ex)
        {
            StackLog = $"Не удалось создать файл: {ex.Message}";
        }
    }

    /// <summary>Читает input.txt, выполняет команды и показывает результат каждой из них.</summary>
    [RelayCommand]
    private async Task RunStackOperationsAsync()
    {
        if (IsStackBusy) return;
        IsStackBusy = true;

        try
        {
            var path = InputFilePath;
            var text = await Task.Run(() => File.ReadAllText(path));
            var result = await Task.Run(() => StackCommandProcessor.Execute(text));

            StackLog = StackCommandProcessor.FormatLog(result);
            StackFinalState = $"Стек (сверху вниз): {result.FinalStack}";
        }
        catch (Exception ex)
        {
            StackLog = $"Не удалось прочитать файл «{InputFilePath}»:\n{ex.Message}";
            StackFinalState = "Стек пуст.";
        }
        finally
        {
            IsStackBusy = false;
        }
    }

    /// <summary>Замеряет чтение и выполнение именно текущего файла.</summary>
    [RelayCommand]
    private async Task MeasureCurrentInputFileAsync()
    {
        if (IsStackBusy) return;
        IsStackBusy = true;

        try
        {
            var path = InputFilePath;
            var (averageMs, operations) = await Task.Run(() => StackBenchmarkService.MeasureFile(path));

            var builder = new StringBuilder();
            builder.AppendLine($"Файл: {path}");
            builder.AppendLine($"Команд выполнено: {operations}");
            builder.AppendLine($"Среднее время чтения и выполнения: {averageMs:F4} мс (5 прогонов)");

            StackLog = builder.ToString();
            StackFinalState = "Замер выполнен.";
        }
        catch (Exception ex)
        {
            StackLog = $"Не удалось выполнить замер файла «{InputFilePath}»:\n{ex.Message}";
        }
        finally
        {
            IsStackBusy = false;
        }
    }

    /// <summary>Строит график времени для сгенерированных наборов операций.</summary>
    [RelayCommand]
    private async Task RunStackBenchmarkAsync()
    {
        if (IsStackBusy) return;
        IsStackBusy = true;

        try
        {
            var points = await Task.Run(() => StackBenchmarkService.MeasureCommandExecution(
                StackBenchmarkStartN, StackBenchmarkMaxN, StackBenchmarkStepN));

            var chartData = new ChartDataModel();
            var approximation = new ApproximationService().Fit(points, ComplexityType.Linear);
            chartData.SeriesList.Add(new SeriesData(
                "Чтение файла + операции стека", points, approximation));

            new ChartWindow
            {
                DataContext = new ChartWindowViewModel(
                    chartData,
                    "Время считывания и выполнения операций стека",
                    "Количество операций (N)",
                    "Время (мс)")
            }.Show();
        }
        catch (Exception ex)
        {
            StackLog = $"Ошибка замера операций стека: {ex.Message}";
        }
        finally
        {
            IsStackBusy = false;
        }
    }

    /// <summary>Создаёт postfix.txt с примером корректного выражения.</summary>
    [RelayCommand]
    private void CreateSamplePostfixFile()
    {
        try
        {
            File.WriteAllText(PostfixFilePath, SamplePostfix, Encoding.UTF8);
            PostfixLog = $"Создан файл:\n{PostfixFilePath}\n\nСодержимое:\n{SamplePostfix}";
            PostfixResult = "—";
        }
        catch (Exception ex)
        {
            PostfixLog = $"Не удалось создать файл: {ex.Message}";
        }
    }

    /// <summary>Читает постфиксное выражение из файла и вычисляет его.</summary>
    [RelayCommand]
    private async Task EvaluatePostfixAsync()
    {
        if (IsStackBusy) return;
        IsStackBusy = true;

        try
        {
            var path = PostfixFilePath;
            var text = await Task.Run(() => File.ReadAllText(path));
            var result = await Task.Run(() => PostfixEvaluator.Evaluate(text));

            var builder = new StringBuilder();
            builder.AppendLine($"Выражение: {text.Trim()}");
            builder.AppendLine();

            foreach (var step in result.Steps) builder.AppendLine(step);

            builder.AppendLine();
            builder.Append(result.Success
                ? $"Результат: {Format(result.Value)}"
                : $"Ошибка: {result.Error}");

            PostfixLog = builder.ToString();
            PostfixResult = result.Success ? Format(result.Value) : "ошибка";
            PostfixComplexity = result.TokenCount > 0
                ? $"Токенов в выражении: {result.TokenCount}. Теоретическая оценка: O(n) по времени и O(n) по памяти."
                : "Теоретическая оценка: O(n) по времени и O(n) по памяти, где n — число токенов выражения.";
        }
        catch (Exception ex)
        {
            PostfixLog = $"Не удалось прочитать файл «{PostfixFilePath}»:\n{ex.Message}";
            PostfixResult = "ошибка";
        }
        finally
        {
            IsStackBusy = false;
        }
    }

    /// <summary>Строит эмпирическую оценку сложности вычисления постфиксной записи.</summary>
    [RelayCommand]
    private async Task RunPostfixBenchmarkAsync()
    {
        if (IsStackBusy) return;
        IsStackBusy = true;

        try
        {
            var points = await Task.Run(() => StackBenchmarkService.MeasurePostfixEvaluation(
                PostfixBenchmarkStartN, PostfixBenchmarkMaxN, PostfixBenchmarkStepN));

            var chartData = new ChartDataModel();
            var approximation = new ApproximationService().Fit(points, ComplexityType.Linear);
            chartData.SeriesList.Add(new SeriesData(
                "Вычисление постфиксной записи", points, approximation));

            new ChartWindow
            {
                DataContext = new ChartWindowViewModel(
                    chartData,
                    "Оценка сложности вычисления постфиксной записи",
                    "Число токенов (N)",
                    "Время (мс)")
            }.Show();
        }
        catch (Exception ex)
        {
            PostfixLog = $"Ошибка замера постфиксной записи: {ex.Message}";
        }
        finally
        {
            IsStackBusy = false;
        }
    }

    private static string Format(double value) =>
        value.ToString("G8", CultureInfo.InvariantCulture);
}
