using System.Collections.Generic;

namespace Algorithms.Lab3.LinkedLists;

/// <summary>
///     Функции работы с собственным односвязным списком.
///     Там, где это требуется по заданию, ссылки меняются напрямую.
/// </summary>
public static class LinkedListAlgorithms
{
    /// <summary>1. Переворачивает список, меняя ссылки между узлами.</summary>
    public static void Reverse<T>(SinglyLinkedList<T> list)
    {
        ListNode<T>? previous = null;
        var current = list.Head;

        while (current is not null)
        {
            var next = current.Next;
            current.Next = previous;
            previous = current;
            current = next;
        }

        list.Head = previous;
    }

    /// <summary>2а. Переносит последний элемент непустого списка в начало.</summary>
    public static void MoveLastToFront<T>(SinglyLinkedList<T> list)
    {
        var head = list.Head;
        if (head?.Next is null) return;

        var previous = head;
        var last = head.Next;

        while (last.Next is not null)
        {
            previous = last;
            last = last.Next;
        }

        previous.Next = null;
        last.Next = head;
        list.Head = last;
    }

    /// <summary>2б. Переносит первый элемент непустого списка в конец.</summary>
    public static void MoveFirstToLast<T>(SinglyLinkedList<T> list)
    {
        var head = list.Head;
        if (head?.Next is null) return;

        list.Head = head.Next;

        var tail = list.Head!;
        while (tail.Next is not null) tail = tail.Next;

        tail.Next = head;
        head.Next = null;
    }

    /// <summary>3. Количество различных элементов списка целых чисел.</summary>
    public static int CountDistinct(SinglyLinkedList<int> list)
    {
        var seen = new HashSet<int>();
        foreach (var value in list.Values()) seen.Add(value);
        return seen.Count;
    }

    /// <summary>
    ///     4. Удаляет неуникальные элементы, то есть оставляет только значения,
    ///     встречающиеся в списке ровно один раз.
    /// </summary>
    public static void RemoveNonUnique(SinglyLinkedList<int> list)
    {
        var counts = new Dictionary<int, int>();
        foreach (var value in list.Values())
            counts[value] = counts.GetValueOrDefault(value) + 1;

        ListNode<int>? previous = null;
        var current = list.Head;

        while (current is not null)
        {
            var next = current.Next;

            if (counts[current.Value] > 1)
            {
                if (previous is null)
                    list.Head = next;
                else
                    previous.Next = next;
            }
            else
            {
                previous = current;
            }

            current = next;
        }
    }

    /// <summary>
    ///     5. Вставляет копию списка в себя сразу после первого вхождения числа x.
    ///     Возвращает false, если x не найден.
    /// </summary>
    public static bool InsertSelfAfterFirst(SinglyLinkedList<int> list, int x)
    {
        var target = Find(list, x);
        if (target is null) return false;

        // Копия обязательна: вставка тех же узлов замкнула бы список в цикл.
        var copy = list.Clone();
        if (copy.Head is null) return false;

        var copyTail = copy.Head;
        while (copyTail.Next is not null) copyTail = copyTail.Next;

        copyTail.Next = target.Next;
        target.Next = copy.Head;
        return true;
    }

    /// <summary>
    ///     6. Вставляет E в список, упорядоченный по неубыванию, сохраняя порядок.
    /// </summary>
    public static void InsertSorted(SinglyLinkedList<int> list, int value)
    {
        var node = new ListNode<int>(value);

        if (list.Head is null || value <= list.Head.Value)
        {
            node.Next = list.Head;
            list.Head = node;
            return;
        }

        var current = list.Head;
        while (current.Next is not null && current.Next.Value < value) current = current.Next;

        node.Next = current.Next;
        current.Next = node;
    }

    /// <summary>7. Удаляет все элементы со значением E. Возвращает число удалённых.</summary>
    public static int RemoveAll(SinglyLinkedList<int> list, int value)
    {
        var removed = 0;
        ListNode<int>? previous = null;
        var current = list.Head;

        while (current is not null)
        {
            var next = current.Next;

            if (current.Value == value)
            {
                if (previous is null)
                    list.Head = next;
                else
                    previous.Next = next;

                removed++;
            }
            else
            {
                previous = current;
            }

            current = next;
        }

        return removed;
    }

    /// <summary>
    ///     8. Вставляет F перед первым вхождением E. Возвращает false, если E нет в списке.
    /// </summary>
    public static bool InsertBeforeFirst(SinglyLinkedList<int> list, int target, int newValue)
    {
        ListNode<int>? previous = null;
        var current = list.Head;

        while (current is not null && current.Value != target)
        {
            previous = current;
            current = current.Next;
        }

        if (current is null) return false;

        var node = new ListNode<int>(newValue) { Next = current };

        if (previous is null)
            list.Head = node;
        else
            previous.Next = node;

        return true;
    }

    /// <summary>
    ///     9. Дописывает в конец target элементы списка source.
    ///     Значения копируются, поэтому source остаётся неизменным.
    /// </summary>
    public static void Append(SinglyLinkedList<int> target, SinglyLinkedList<int> source)
    {
        // Снимок нужен на случай, если target и source — один и тот же список.
        var snapshot = new List<int>(source.Values());
        foreach (var value in snapshot) target.AddLast(value);
    }

    /// <summary>
    ///     10. Разбивает список по первому вхождению value.
    ///     Первый список содержит элементы до вхождения включительно,
    ///     второй — остаток. Если value нет, второй список пуст.
    /// </summary>
    public static (SinglyLinkedList<int> First, SinglyLinkedList<int> Second) SplitAtFirst(
        SinglyLinkedList<int> list,
        int value)
    {
        var first = new SinglyLinkedList<int>();
        var second = new SinglyLinkedList<int>();
        var found = false;

        foreach (var item in list.Values())
        {
            if (!found)
            {
                first.AddLast(item);
                if (item == value) found = true;
            }
            else
            {
                second.AddLast(item);
            }
        }

        return found
            ? (first, second)
            : (list.Clone(), new SinglyLinkedList<int>());
    }

    /// <summary>11. Удваивает список: приписывает в конец его копию.</summary>
    public static void Double(SinglyLinkedList<int> list)
    {
        var snapshot = new List<int>(list.Values());
        foreach (var value in snapshot) list.AddLast(value);
    }

    /// <summary>
    ///     12. Меняет местами значения двух элементов по позициям (нумерация с 1).
    /// </summary>
    public static bool SwapAtPositions<T>(SinglyLinkedList<T> list, int firstPosition, int secondPosition)
    {
        if (firstPosition < 1 || secondPosition < 1) return false;
        if (firstPosition == secondPosition) return true;

        var first = NodeAt(list, firstPosition);
        var second = NodeAt(list, secondPosition);

        if (first is null || second is null) return false;

        (first.Value, second.Value) = (second.Value, first.Value);
        return true;
    }

    /// <summary>Первый узел со значением value или null.</summary>
    private static ListNode<T>? Find<T>(SinglyLinkedList<T> list, T value)
    {
        for (var node = list.Head; node is not null; node = node.Next)
            if (EqualityComparer<T>.Default.Equals(node.Value, value))
                return node;

        return null;
    }

    /// <summary>Узел на позиции (нумерация с 1) или null.</summary>
    private static ListNode<T>? NodeAt<T>(SinglyLinkedList<T> list, int position)
    {
        var node = list.Head;

        for (var i = 1; node is not null && i < position; i++)
            node = node.Next;

        return node;
    }
}
