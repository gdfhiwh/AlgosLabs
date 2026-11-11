using System.Collections.Generic;
using System.Text;

namespace Algorithms.Lab3.Queue;

/// <summary>
///     Очередь на стандартном классе <see cref="System.Collections.Generic.Queue{T}" />.
///     Используется для сравнения со списковой реализацией.
/// </summary>
public sealed class StandardQueue<T> : IQueue<T>
{
    private readonly System.Collections.Generic.Queue<T> _queue = new();

    public int Count => _queue.Count;

    public bool IsEmpty => _queue.Count == 0;

    public void Enqueue(T item) => _queue.Enqueue(item);

    public T Dequeue() => _queue.Dequeue();

    public T Peek() => _queue.Peek();

    public IEnumerable<T> FrontToBack() => _queue;

    public string Print()
    {
        var builder = new StringBuilder("[");
        var first = true;

        foreach (var value in _queue)
        {
            if (!first) builder.Append(' ');
            builder.Append(value);
            first = false;
        }

        builder.Append(']');
        return builder.ToString();
    }
}
