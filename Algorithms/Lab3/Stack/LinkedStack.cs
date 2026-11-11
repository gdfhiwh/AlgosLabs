using System.Collections.Generic;
using System.Text;

namespace Algorithms.Lab3.Stack;

/// <summary>
///     Стек, реализованный на собственном односвязном списке.
///     Стандартные коллекции (Stack, List и т.п.) не используются.
/// </summary>
public sealed class LinkedStack<T>
{
    /// <summary>Узел односвязного списка: значение и ссылка на следующий узел.</summary>
    private sealed class Node
    {
        public Node(T value) => Value = value;

        public T Value { get; }
        public Node? Next { get; set; }
    }

    private Node? _head;

    /// <summary>Количество элементов в стеке.</summary>
    public int Count { get; private set; }

    /// <summary>Пуст ли стек.</summary>
    public bool IsEmpty => _head is null;

    /// <summary>Push(elem): добавляет элемент на вершину стека.</summary>
    public void Push(T item)
    {
        _head = new Node(item) { Next = _head };
        Count++;
    }

    /// <summary>Pop(): снимает и возвращает верхний элемент.</summary>
    public T Pop()
    {
        if (_head is null) throw new InvalidOperationException("Стек пуст: операция Pop невозможна.");

        var value = _head.Value;
        _head = _head.Next;
        Count--;
        return value;
    }

    /// <summary>Top(): возвращает верхний элемент, не снимая его.</summary>
    public T Top()
    {
        if (_head is null) throw new InvalidOperationException("Стек пуст: операция Top невозможна.");

        return _head.Value;
    }

    /// <summary>Перебор элементов от вершины к основанию.</summary>
    public IEnumerable<T> TopToBottom()
    {
        for (var node = _head; node is not null; node = node.Next)
            yield return node.Value;
    }

    /// <summary>Print(): строковое представление стека от вершины к основанию.</summary>
    public string Print()
    {
        var builder = new StringBuilder("[");
        var first = true;

        foreach (var value in TopToBottom())
        {
            if (!first) builder.Append(' ');
            builder.Append(value);
            first = false;
        }

        builder.Append(']');
        return builder.ToString();
    }
}
