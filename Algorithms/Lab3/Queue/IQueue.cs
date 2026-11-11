using System.Collections.Generic;

namespace Algorithms.Lab3.Queue;

/// <summary>
///     Общий интерфейс очереди, чтобы команды из файла и замеры работали
///     с любой реализацией одинаково.
/// </summary>
public interface IQueue<T>
{
    int Count { get; }
    bool IsEmpty { get; }

    /// <summary>Вставка элемента в конец очереди.</summary>
    void Enqueue(T item);

    /// <summary>Удаление и возврат первого элемента.</summary>
    T Dequeue();

    /// <summary>Просмотр первого элемента без удаления.</summary>
    T Peek();

    /// <summary>Перебор элементов от начала очереди к концу.</summary>
    IEnumerable<T> FrontToBack();

    /// <summary>Печать очереди от начала к концу.</summary>
    string Print();
}
