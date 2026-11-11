namespace Algorithms.Lab3.LinkedLists;

/// <summary>Узел собственного односвязного списка.</summary>
public sealed class ListNode<T>
{
    public ListNode(T value) => Value = value;

    public T Value { get; set; }
    public ListNode<T>? Next { get; set; }
}
