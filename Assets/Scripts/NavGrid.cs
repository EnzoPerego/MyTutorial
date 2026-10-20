using System.Collections.Generic;
using UnityEngine;

// Grade de navegação dos piratas. Um campo de fluxo (BFS a partir do alvo) diz,
// para cada célula de água, qual vizinha leva mais rápido até o surfista contornando ilhas.
public class NavGrid
{
    public static readonly Vector2Int[] Cardinal =
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left
    };

    private static readonly Vector2Int[] Neighbours =
    {
        Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left,
        new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1), new Vector2Int(-1, 1)
    };

    public const int Unreachable = int.MaxValue;

    public readonly int width;
    public readonly int height;
    private readonly bool[,] walkable;
    private readonly int[,] distance;
    private Vector2Int target = new Vector2Int(-1, -1);

    public NavGrid(LevelLayout layout)
    {
        width = layout.width;
        height = layout.height;
        walkable = new bool[width, height];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                walkable[x, y] = !layout.IsBlocked(x, y);
        distance = new int[width, height];
    }

    public Vector2Int Target
    {
        get { return target; }
    }

    public bool IsWalkable(Vector2Int c)
    {
        return c.x >= 0 && c.y >= 0 && c.x < width && c.y < height && walkable[c.x, c.y];
    }

    // Diagonais só valem se as duas células laterais também forem água (não corta quina de ilha).
    private bool CanStep(Vector2Int from, Vector2Int d)
    {
        if (!IsWalkable(from + d)) return false;
        if (d.x == 0 || d.y == 0) return true;
        return IsWalkable(new Vector2Int(from.x + d.x, from.y)) && IsWalkable(new Vector2Int(from.x, from.y + d.y));
    }

    public void BuildFlowField(Vector2Int newTarget)
    {
        if (newTarget == target || !IsWalkable(newTarget)) return;
        target = newTarget;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                distance[x, y] = Unreachable;

        var queue = new Queue<Vector2Int>();
        distance[target.x, target.y] = 0;
        queue.Enqueue(target);
        while (queue.Count > 0)
        {
            Vector2Int c = queue.Dequeue();
            int next = distance[c.x, c.y] + 1;
            foreach (Vector2Int d in Neighbours)
            {
                Vector2Int n = c + d;
                if (CanStep(c, d) && distance[n.x, n.y] == Unreachable)
                {
                    distance[n.x, n.y] = next;
                    queue.Enqueue(n);
                }
            }
        }
    }

    public int DistanceTo(Vector2Int c)
    {
        return IsWalkable(c) ? distance[c.x, c.y] : Unreachable;
    }

    // Próxima célula no caminho até o alvo. Se a célula atual estiver bloqueada
    // (navio encostado numa ilha), procura a vizinha de água mais próxima do alvo.
    public bool TryGetNextCell(Vector2Int from, out Vector2Int next)
    {
        next = from;
        int best = IsWalkable(from) ? distance[from.x, from.y] : Unreachable;
        bool found = false;
        foreach (Vector2Int d in Neighbours)
        {
            Vector2Int n = from + d;
            bool allowed = IsWalkable(from) ? CanStep(from, d) : IsWalkable(n);
            if (!allowed) continue;
            int dist = distance[n.x, n.y];
            if (dist < best)
            {
                best = dist;
                next = n;
                found = true;
            }
        }
        return found;
    }

    public List<Vector2Int> ReachableCells(Vector2Int from)
    {
        var result = new List<Vector2Int>();
        if (!IsWalkable(from)) return result;

        var seen = new bool[width, height];
        var queue = new Queue<Vector2Int>();
        seen[from.x, from.y] = true;
        queue.Enqueue(from);
        while (queue.Count > 0)
        {
            Vector2Int c = queue.Dequeue();
            result.Add(c);
            foreach (Vector2Int d in Neighbours)
            {
                Vector2Int n = c + d;
                if (CanStep(c, d) && !seen[n.x, n.y])
                {
                    seen[n.x, n.y] = true;
                    queue.Enqueue(n);
                }
            }
        }
        return result;
    }
}
