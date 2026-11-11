using System.Collections.Generic;
using System.Text;

namespace Algorithms.Lab3.LinkedLists;

/// <summary>
///     Собственный односвязный список. Алгоритмы работы с ним находятся
///     в <see cref="LinkedListAlgorithms" /> и меняют ссылки напрямую.
/// </summary>
public sealed class SinglyLinkedList<T>
{
    public ListNode<T>? Head { get; set; }

    public bool IsEmpty => Head is null;

    /// <summary>Количество элементов (считается обходом, чтобы не рассинхронизироваться).</summary>
    public int Count
    {
        get
        {
            var count = 0;
            for (var node = Head; node is not null; node = node.Next) count++;
            return count;
        }
    }

    public void AddFirst(T value) => Head = new ListNode<T>(value) { Next = Head };

    public void AddLast(T value)
    {
        var node = new ListNode<T>(value);

        if (Head is null)
        {
            Head = node;
            return;
        }

        var tail = Head;
        while (tail.Next is not null) tail = tail.Next;
        tail.Next = node;
    }

    /// <summary>Перебор значений от начала к концу.</summary>
    public IEnumerable<T> Values()
    {
        for (var node = Head; node is not null; node = node.Next)
            yield return node.Value;
    }

    public List<T> ToList() => new(Values());

    /// <summary>Копия списка (значения копируются в новые узлы).</summary>
    public SinglyLinkedList<T> Clone()
    {
        var copy = new SinglyLinkedList<T>();
        foreach (var value in Values()) copy.AddLast(value);
        return copy;
    }

    public static SinglyLinkedList<T> FromValues(IEnumerable<T> values)
    {
        var list = new SinglyLinkedList<T>();
        foreach (var value in values) list.AddLast(value);
        return list;
    }

    public override string ToString()
    {
        var builder = new StringBuilder("[");
        var first = true;

        foreach (var value in Values())
        {
            if (!first) builder.Append(' ');
            builder.Append(value);
            first = false;
        }

        builder.Append(']');
        return builder.ToString();
    }
}
