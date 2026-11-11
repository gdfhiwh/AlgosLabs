using Algorithms.Lab3.Queue;

namespace Algorithms.Lab3.DynamicStructures;

/// <summary>Порядок обхода и расстояния от стартовой вершины.</summary>
public sealed record BfsResult(IReadOnlyList<int> Order, IReadOnlyList<int> Distances);

/// <summary>
///     Пример применения очереди: обход графа в ширину (BFS).
///     FIFO гарантирует, что вершины обрабатываются в порядке неубывания расстояния
///     от старта, поэтому каждое расстояние вычисляется за один проход.
///     Время — O(V + E), память — O(V).
/// </summary>
public static class BreadthFirstSearch
{
    public static BfsResult Run(IReadOnlyList<IReadOnlyList<int>> adjacency, int start)
    {
        if (adjacency.Count == 0)
            throw new ArgumentException("Граф пуст.", nameof(adjacency));

        if (start < 0 || start >= adjacency.Count)
            throw new ArgumentOutOfRangeException(nameof(start), "Стартовая вершина вне графа.");

        var distances = new int[adjacency.Count];
        Array.Fill(distances, -1);

        var order = new List<int>(adjacency.Count);

        // Собственная очередь на списке из части 2 — ещё одна динамическая структура.
        var queue = new ListQueue<int>();

        distances[start] = 0;
        queue.Enqueue(start);

        while (!queue.IsEmpty)
        {
            var vertex = queue.Dequeue();
            order.Add(vertex);

            foreach (var neighbor in adjacency[vertex])
            {
                if (distances[neighbor] != -1) continue;

                distances[neighbor] = distances[vertex] + 1;
                queue.Enqueue(neighbor);
            }
        }

        return new BfsResult(order, distances);
    }
}
