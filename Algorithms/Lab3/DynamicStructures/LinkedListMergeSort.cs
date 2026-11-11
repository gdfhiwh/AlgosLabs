using Algorithms.Lab3.LinkedLists;

namespace Algorithms.Lab3.DynamicStructures;

/// <summary>
///     Пример применения списка: сортировка слиянием односвязного списка.
///     Список не поддерживает случайный доступ, поэтому вместо разбиения по индексам
///     используется поиск середины двумя указателями и слияние готовых цепочек.
///     Время — O(n log n), дополнительная память — O(log n) на рекурсию.
/// </summary>
public static class LinkedListMergeSort
{
    public static void Sort(SinglyLinkedList<int> list) => list.Head = Sort(list.Head);

    private static ListNode<int>? Sort(ListNode<int>? head)
    {
        if (head?.Next is null) return head;

        var (left, right) = Split(head);
        return Merge(Sort(left), Sort(right));
    }

    /// <summary>Делит список пополам указателями «медленный / быстрый».</summary>
    private static (ListNode<int>? Left, ListNode<int>? Right) Split(ListNode<int> head)
    {
        var slow = head;
        var fast = head.Next;

        while (fast?.Next is not null)
        {
            slow = slow.Next!;
            fast = fast.Next.Next;
        }

        var right = slow.Next;
        slow.Next = null;
        return (head, right);
    }

    /// <summary>Сливает две упорядоченные цепочки, переставляя ссылки.</summary>
    private static ListNode<int>? Merge(ListNode<int>? left, ListNode<int>? right)
    {
        var dummy = new ListNode<int>(0);
        var tail = dummy;

        while (left is not null && right is not null)
        {
            if (left.Value <= right.Value)
            {
                tail.Next = left;
                left = left.Next;
            }
            else
            {
                tail.Next = right;
                right = right.Next;
            }

            tail = tail.Next;
        }

        tail.Next = left ?? right;
        return dummy.Next;
    }
}
