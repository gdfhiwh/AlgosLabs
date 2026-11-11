using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Algorithms.Lab3.DynamicStructures;
using Algorithms.Lab3.LinkedLists;
using Algorithms.Lab3.Stack;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AlgosLabs.ViewModels;

/// <summary>
///     Раздел «Динамические структуры»: четыре примера применения —
///     список (сортировка слиянием), стек (RPN), очередь (BFS), дерево (Trie).
/// </summary>
public partial class DynamicStructuresViewModel : ObservableObject
{
    // 1. Список — сортировка слиянием связного списка.

    [ObservableProperty] private string _mergeSortInput = "5 2 9 1 5 6 3 8 4 7";
    [ObservableProperty] private string _mergeSortResult = "—";

    // 2. Стек — вычисление выражения в обратной польской записи.

    [ObservableProperty] private string _rpnExpression = "5 1 2 + 4 * + 3 -";
    [ObservableProperty] private string _rpnResult = "—";
    [ObservableProperty] private string _rpnSteps = "Выражение ещё не вычислялось.";

    // 3. Очередь — обход графа в ширину.

    [ObservableProperty]
    private string _bfsGraph = "0: 1 2\n1: 0 3\n2: 0 4\n3: 1 4 5\n4: 2 3 5\n5: 3 4";

    [ObservableProperty] private int _bfsStart;
    [ObservableProperty] private string _bfsOrder = "—";
    [ObservableProperty] private string _bfsDistances = "—";
    [ObservableProperty] private string _bfsInfo = "Граф ещё не обойдён.";

    // 4. Дерево — Trie и автодополнение по префиксу.

    [ObservableProperty] private string _trieWords = "car cart carbon care cat dog dart door";
    [ObservableProperty] private string _triePrefix = "car";
    [ObservableProperty] private string _trieSuggestions = "—";
    [ObservableProperty] private string _trieInfo = "Введите слова и префикс.";

    /// <summary>Сортирует связный список слиянием.</summary>
    [RelayCommand]
    private void RunMergeSort()
    {
        if (!TryParseInts(MergeSortInput, out var values))
        {
            MergeSortResult = "Ожидаются целые числа через пробел или запятую.";
            return;
        }

        var list = SinglyLinkedList<int>.FromValues(values);
        LinkedListMergeSort.Sort(list);
        MergeSortResult = list.ToString();
    }

    /// <summary>Вычисляет выражение в обратной польской записи на стеке.</summary>
    [RelayCommand]
    private void RunRpn()
    {
        var result = PostfixEvaluator.Evaluate(RpnExpression);

        RpnResult = result.Success
            ? result.Value.ToString("G8", CultureInfo.InvariantCulture)
            : "ошибка";

        var builder = new StringBuilder();
        foreach (var step in result.Steps) builder.AppendLine(step);

        if (!result.Success)
        {
            builder.AppendLine();
            builder.Append("Ошибка: ").Append(result.Error);
        }

        RpnSteps = builder.Length == 0 ? "Выражение пустое." : builder.ToString();
    }

    /// <summary>Обходит граф в ширину собственной очередью.</summary>
    [RelayCommand]
    private void RunBfs()
    {
        if (!TryParseGraph(BfsGraph, out var adjacency))
        {
            BfsOrder = "—";
            BfsDistances = "—";
            BfsInfo = "Не удалось разобрать граф. Формат строки: «0: 1 2».";
            return;
        }

        if (BfsStart < 0 || BfsStart >= adjacency.Count)
        {
            BfsOrder = "—";
            BfsDistances = "—";
            BfsInfo = $"Стартовая вершина должна быть от 0 до {adjacency.Count - 1}.";
            return;
        }

        var result = BreadthFirstSearch.Run(adjacency, BfsStart);

        BfsOrder = result.Order.Count == 0 ? "—" : string.Join(" → ", result.Order);
        BfsDistances = string.Join(", ",
            result.Distances.Select((distance, vertex) =>
                $"{vertex}: {(distance < 0 ? "∞" : distance.ToString(CultureInfo.InvariantCulture))}"));

        BfsInfo = $"Достижимо вершин: {result.Order.Count} из {adjacency.Count}.";
    }

    /// <summary>Строит Trie и показывает автодополнение по префиксу.</summary>
    [RelayCommand]
    private void RunAutocomplete()
    {
        var words = TrieWords.Split(
            new[] { ' ', ',', ';', '\t', '\r', '\n' },
            StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            TrieSuggestions = "—";
            TrieInfo = "Введите хотя бы одно слово.";
            return;
        }

        var trie = new Trie();
        foreach (var word in words) trie.Insert(word);

        var count = trie.CountWithPrefix(TriePrefix);
        var suggestions = trie.Complete(TriePrefix, 10);

        TrieInfo = $"Слов в дереве: {trie.WordCount}. Начинаются с «{TriePrefix}»: {count}.";
        TrieSuggestions = suggestions.Count == 0
            ? "Совпадений нет."
            : string.Join(", ", suggestions) + (count > suggestions.Count ? " …" : string.Empty);
    }

    private static bool TryParseInts(string text, out List<int> values)
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

    /// <summary>
    ///     Разбирает граф в формате «вершина: сосед сосед» по одной строке на вершину.
    /// </summary>
    private static bool TryParseGraph(string text, out List<List<int>> adjacency)
    {
        adjacency = new List<List<int>>();
        var edges = new Dictionary<int, List<int>>();
        var maxVertex = -1;

        foreach (var rawLine in text.Split('\n'))
        {
            var line = rawLine.Trim();
            if (line.Length == 0) continue;

            var colon = line.IndexOf(':');
            if (colon < 0) return false;

            if (!int.TryParse(line[..colon].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                    out var vertex) || vertex < 0)
                return false;

            var neighbors = new List<int>();
            var rest = line[(colon + 1)..];

            foreach (var part in rest.Split(
                         new[] { ' ', ',', ';', '\t' },
                         StringSplitOptions.RemoveEmptyEntries))
            {
                if (!int.TryParse(part, NumberStyles.Integer, CultureInfo.InvariantCulture,
                        out var neighbor) || neighbor < 0)
                    return false;

                neighbors.Add(neighbor);
                maxVertex = Math.Max(maxVertex, neighbor);
            }

            edges[vertex] = neighbors;
            maxVertex = Math.Max(maxVertex, vertex);
        }

        if (maxVertex < 0) return false;

        adjacency = Enumerable.Range(0, maxVertex + 1)
            .Select(vertex => edges.TryGetValue(vertex, out var list) ? list : new List<int>())
            .ToList();

        return true;
    }
}
