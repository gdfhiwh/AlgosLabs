using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Algorithms.Lab3.LinkedLists;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgosLabs.ViewModels;

/// <summary>
///     Раздел «Связный список» третьей лабы: 12 операций над собственным
///     односвязным списком. Для операции 9 оба списка можно считать из файла.
/// </summary>
public partial class LinkedListViewModel : ObservableObject
{
    public LinkedListViewModel()
    {
        _listsFilePath = Path.Combine(AppContext.BaseDirectory, "lists.txt");
        ApplyInputs();
    }

    // Ввод списков.

    [ObservableProperty] private string _listText = "1 2 3 2 4 5";
    [ObservableProperty] private string _listEText = "10 20 30";
    [ObservableProperty] private string _listsFilePath;

    // Текущее состояние и результат.

    [ObservableProperty] private string _currentList = "[]";
    [ObservableProperty] private string _currentListE = "[]";
    [ObservableProperty] private string _resultMessage = "Выберите операцию и нажмите «Выполнить».";

    // Выбранная операция и параметры.

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NeedsX))]
    [NotifyPropertyChangedFor(nameof(NeedsE))]
    [NotifyPropertyChangedFor(nameof(NeedsF))]
    [NotifyPropertyChangedFor(nameof(NeedsPositions))]
    [NotifyPropertyChangedFor(nameof(SelectedOperationDescription))]
    private int _operationIndex;

    [ObservableProperty] private int _parameterX = 3;
    [ObservableProperty] private int _parameterE = 5;
    [ObservableProperty] private int _parameterF = 99;
    [ObservableProperty] private int _positionFirst = 1;
    [ObservableProperty] private int _positionSecond = 3;

    public IReadOnlyList<string> OperationNames { get; } = new[]
    {
        "1. Перевернуть список",
        "2. Последний элемент → в начало",
        "3. Первый элемент → в конец",
        "4. Количество различных элементов",
        "5. Удалить неуникальные элементы",
        "6. Вставить список в себя после первого x",
        "7. Вставить E с сохранением порядка",
        "8. Удалить все элементы E",
        "9. Вставить F перед первым E",
        "10. Дописать список E",
        "11. Разбить по первому вхождению x",
        "12. Удвоить список",
        "13. Поменять местами элементы на позициях"
    };

    private static readonly string[] OperationDescriptions =
    {
        "Меняет ссылки так, чтобы элементы шли в обратном порядке.",
        "Переносит последний элемент непустого списка в начало.",
        "Переносит первый элемент непустого списка в конец.",
        "Считает, сколько различных чисел встречается в списке.",
        "Оставляет только элементы, встречающиеся ровно один раз.",
        "Вставляет копию списка сразу после первого узла со значением x.",
        "Вставляет E в список, упорядоченный по неубыванию, сохраняя порядок.",
        "Удаляет из списка все узлы со значением E.",
        "Вставляет F перед первым узлом со значением E, если E есть в списке.",
        "Дописывает элементы списка E в конец L; список E не меняется.",
        "Первый список — элементы до первого x включительно, второй — остаток.",
        "Приписывает в конец копию самого списка.",
        "Меняет местами значения на позициях; нумерация с 1."
    };

    public string SelectedOperationDescription =>
        OperationDescriptions[Math.Clamp(OperationIndex, 0, OperationDescriptions.Length - 1)];

    public bool NeedsX => OperationIndex is 5 or 10;
    public bool NeedsE => OperationIndex is 6 or 7 or 8;
    public bool NeedsF => OperationIndex == 8;
    public bool NeedsPositions => OperationIndex == 12;

    /// <summary>Создаёт lists.txt с двумя строками: список L и список E.</summary>
    [RelayCommand]
    private void CreateSampleFile()
    {
        try
        {
            File.WriteAllText(ListsFilePath, "1 2 3 2 4 5\n10 20 30", Encoding.UTF8);
            ListText = "1 2 3 2 4 5";
            ListEText = "10 20 30";
            ApplyInputs();
            ResultMessage = $"Создан файл:\n{ListsFilePath}";
        }
        catch (Exception ex)
        {
            ResultMessage = $"Не удалось создать файл: {ex.Message}";
        }
    }

    /// <summary>Читает два списка из файла: первая строка — L, вторая — E.</summary>
    [RelayCommand]
    private void LoadListsFromFile()
    {
        try
        {
            if (!File.Exists(ListsFilePath))
            {
                ResultMessage = $"Файл не найден: {ListsFilePath}";
                return;
            }

            var lines = File.ReadAllLines(ListsFilePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            if (lines.Count == 0)
            {
                ResultMessage = "Файл пуст: ожидались строки со списками L и E.";
                return;
            }

            ListText = lines[0];
            ListEText = lines.Count > 1 ? lines[1] : string.Empty;
            ApplyInputs();

            ResultMessage = lines.Count > 1
                ? "Списки L и E загружены из файла."
                : "Загружен только список L: вторая строка отсутствует, список E пуст.";
        }
        catch (Exception ex)
        {
            ResultMessage = $"Не удалось прочитать файл: {ex.Message}";
        }
    }

    /// <summary>Применяет введённые списки и показывает их в текущем состоянии.</summary>
    [RelayCommand]
    private void ApplyInputs()
    {
        if (!TryParse(ListText, out var values))
        {
            ResultMessage = "Список L должен содержать целые числа через пробел или запятую.";
            return;
        }

        if (!TryParse(ListEText, out var valuesE))
        {
            ResultMessage = "Список E должен содержать целые числа через пробел или запятую.";
            return;
        }

        CurrentList = SinglyLinkedList<int>.FromValues(values).ToString();
        CurrentListE = SinglyLinkedList<int>.FromValues(valuesE).ToString();
    }

    /// <summary>Выполняет выбранную операцию над текущим содержимым полей ввода.</summary>
    [RelayCommand]
    private void RunOperation()
    {
        if (!TryParse(ListText, out var values))
        {
            ResultMessage = "Список L должен содержать целые числа через пробел или запятую.";
            return;
        }

        if (!TryParse(ListEText, out var valuesE))
        {
            ResultMessage = "Список E должен содержать целые числа через пробел или запятую.";
            return;
        }

        var list = SinglyLinkedList<int>.FromValues(values);
        var listE = SinglyLinkedList<int>.FromValues(valuesE);
        string message;

        switch (OperationIndex)
        {
            case 0:
                LinkedListAlgorithms.Reverse(list);
                message = "Список перевёрнут.";
                break;

            case 1:
                LinkedListAlgorithms.MoveLastToFront(list);
                message = "Последний элемент перенесён в начало.";
                break;

            case 2:
                LinkedListAlgorithms.MoveFirstToLast(list);
                message = "Первый элемент перенесён в конец.";
                break;

            case 3:
            {
                var distinct = LinkedListAlgorithms.CountDistinct(list);
                message = $"Количество различных элементов: {distinct}.";
                break;
            }

            case 4:
                LinkedListAlgorithms.RemoveNonUnique(list);
                message = "Неуникальные элементы удалены, остались только встречающиеся один раз.";
                break;

            case 5:
                message = LinkedListAlgorithms.InsertSelfAfterFirst(list, ParameterX)
                    ? $"Копия списка вставлена после первого вхождения {ParameterX}."
                    : $"Число {ParameterX} в списке не найдено.";
                break;

            case 6:
                LinkedListAlgorithms.InsertSorted(list, ParameterE);
                message = $"Элемент {ParameterE} вставлен с сохранением порядка.";
                break;

            case 7:
            {
                var removed = LinkedListAlgorithms.RemoveAll(list, ParameterE);
                message = $"Удалено элементов {ParameterE}: {removed}.";
                break;
            }

            case 8:
                message = LinkedListAlgorithms.InsertBeforeFirst(list, ParameterE, ParameterF)
                    ? $"Элемент {ParameterF} вставлен перед первым вхождением {ParameterE}."
                    : $"Число {ParameterE} в списке не найдено.";
                break;

            case 9:
                LinkedListAlgorithms.Append(list, listE);
                message = $"К списку L дописан список E: {listE}.";
                break;

            case 10:
            {
                var (first, second) = LinkedListAlgorithms.SplitAtFirst(list, ParameterX);
                list = first;
                message = $"Первый список: {first}; второй список: {second}.";
                break;
            }

            case 11:
                LinkedListAlgorithms.Double(list);
                message = "Список удвоен: в конец приписана его копия.";
                break;

            case 12:
                message = LinkedListAlgorithms.SwapAtPositions(list, PositionFirst, PositionSecond)
                    ? $"Элементы на позициях {PositionFirst} и {PositionSecond} поменяны местами."
                    : "Не удалось поменять элементы: проверьте позиции.";
                break;

            default:
                message = "Неизвестная операция.";
                break;
        }

        // Синхронизируем поля ввода, чтобы следующая операция продолжала с результата.
        ListText = string.Join(" ", list.Values());
        ListEText = string.Join(" ", listE.Values());
        CurrentList = list.ToString();
        CurrentListE = listE.ToString();
        ResultMessage = message;
    }

    private static bool TryParse(string text, out List<int> values)
    {
        values = new List<int>();
        if (string.IsNullOrWhiteSpace(text)) return true;

        var parts = text.Split(
            new[] { ' ', ',', ';', '\t', '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                return false;

            values.Add(value);
        }

        return true;
    }
}
