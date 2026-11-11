using System.Collections.Generic;

namespace Algorithms.Lab3.DynamicStructures;

/// <summary>
///     Пример применения дерева: префиксное дерево (Trie) для автодополнения.
///     Вставка, проверка слова и поиск вершины по префиксу занимают O(L),
///     где L — длина строки/префикса, независимо от числа сохранённых слов.
/// </summary>
public sealed class Trie
{
    private sealed class Node
    {
        public Dictionary<char, Node> Children { get; } = new();

        /// <summary>Исходное слово, если узел завершает слово; иначе null.</summary>
        public string? Word { get; set; }
    }

    private readonly Node _root = new();

    public int WordCount { get; private set; }

    public void Insert(string word)
    {
        if (string.IsNullOrEmpty(word)) return;

        var node = _root;
        foreach (var symbol in word.ToLowerInvariant())
        {
            if (!node.Children.TryGetValue(symbol, out var next))
            {
                next = new Node();
                node.Children[symbol] = next;
            }

            node = next;
        }

        if (node.Word is null) WordCount++;
        node.Word = word;
    }

    public bool Contains(string word)
    {
        if (string.IsNullOrEmpty(word)) return false;
        return Traverse(word)?.Word is not null;
    }

    /// <summary>Сколько слов начинается с указанного префикса.</summary>
    public int CountWithPrefix(string prefix)
    {
        var node = Traverse(prefix);
        return node is null ? 0 : CountWords(node);
    }

    /// <summary>Автодополнение: до limit слов, начинающихся с префикса.</summary>
    public IReadOnlyList<string> Complete(string prefix, int limit = 10)
    {
        var node = Traverse(prefix);
        var results = new List<string>();
        if (node is not null) Collect(node, results);

        results.Sort(StringComparer.OrdinalIgnoreCase);

        if (results.Count > limit) results.RemoveRange(limit, results.Count - limit);
        return results;
    }

    /// <summary>Спуск по префиксу: O(L).</summary>
    private Node? Traverse(string prefix)
    {
        var node = _root;

        foreach (var symbol in prefix.ToLowerInvariant())
        {
            if (!node.Children.TryGetValue(symbol, out var next)) return null;
            node = next;
        }

        return node;
    }

    private static int CountWords(Node node)
    {
        var count = node.Word is null ? 0 : 1;
        foreach (var child in node.Children.Values) count += CountWords(child);
        return count;
    }

    private static void Collect(Node node, List<string> results)
    {
        if (node.Word is not null) results.Add(node.Word);

        foreach (var child in node.Children.Values) Collect(child, results);
    }
}
