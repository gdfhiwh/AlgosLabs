using System.Collections.Generic;
using System.Text;

namespace Algorithms.Lab3.Queue;

/// <summary>
///     Очередь на основе списка <see cref="List{T}" />.
///     Удаление из начала списка сдвигает все элементы, поэтому при большой
///     глубине очереди такая реализация заметно медленнее стандартной.
/// </summary>
public sealed class ListQueue<T> : IQueue<T>
{
    private readonly List<T> _items = new();

    public int Count => _items.Count;

    public bool IsEmpty => _items.Count == 0;

    public void Enqueue(T item) => _items.Add(item);

    public T Dequeue()
    {
        if (_items.Count == 0) throw new InvalidOperationException("Очередь пуста: удаление невозможно.");

        var value = _items[0];
        _items.RemoveAt(0);
        return value;
    }

    public T Peek()
    {
        if (_items.Count == 0) throw new InvalidOperationException("Очередь пуста: просмотр невозможен.");

        return _items[0];
    }

    public IEnumerable<T> FrontToBack() => _items;

    public string Print()
    {
        var builder = new StringBuilder("[");
        var first = true;

        foreach (var value in _items)
        {
            if (!first) builder.Append(' ');
            builder.Append(value);
            first = false;
        }

        builder.Append(']');
        return builder.ToString();
    }
}
