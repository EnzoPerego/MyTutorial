using System.Collections.Generic;
using UnityEngine;

// Lê o mapa em texto (Assets/Level/baia.txt). A primeira linha do arquivo é o topo do mapa.
// Cada célula ocupa 1 unidade do mundo; a célula (x, y) vai de (x, y) até (x + 1, y + 1).
public class LevelLayout
{
    public const char Water = '.';
    public const char Island = '#';
    public const char Palm = 'p';
    public const char Tower = 'T';
    public const char Wall = 'w';
    public const char Rock = 'r';
    public const char Start = 'S';
    public const char ChaserSpawn = 'P';
    public const char CannonShip = 'C';

    public readonly int width;
    public readonly int height;
    private readonly char[,] cells;

    public LevelLayout(string text)
    {
        var lines = new List<string>();
        foreach (string raw in text.Replace("\r", "").Split('\n'))
        {
            string line = raw.TrimEnd();
            if (line.Length > 0) lines.Add(line);
        }

        height = lines.Count;
        foreach (string line in lines) width = Mathf.Max(width, line.Length);

        cells = new char[width, height];
        for (int row = 0; row < height; row++)
        {
            int y = height - 1 - row;
            for (int x = 0; x < width; x++)
            {
                cells[x, y] = x < lines[row].Length ? lines[row][x] : Water;
            }
        }
    }

    public char this[int x, int y]
    {
        get { return InBounds(x, y) ? cells[x, y] : Island; }
    }

    public bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }

    public static bool IsIslandChar(char c)
    {
        return c == Island || c == Palm || c == Tower || c == Wall;
    }

    public bool IsIsland(int x, int y)
    {
        return InBounds(x, y) && IsIslandChar(cells[x, y]);
    }

    // Células onde nada navega: fora do mapa, ilhas, pedras e navios ancorados.
    public bool IsBlocked(int x, int y)
    {
        if (!InBounds(x, y)) return true;
        char c = cells[x, y];
        return IsIslandChar(c) || c == Rock || c == CannonShip;
    }

    public List<Vector2Int> Find(char c)
    {
        var found = new List<Vector2Int>();
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (cells[x, y] == c) found.Add(new Vector2Int(x, y));
            }
        }
        return found;
    }

    public Vector2Int FindStart()
    {
        List<Vector2Int> starts = Find(Start);
        return starts.Count > 0 ? starts[0] : new Vector2Int(width / 2, height / 2);
    }

    public static Vector2 CellCenter(Vector2Int cell)
    {
        return new Vector2(cell.x + 0.5f, cell.y + 0.5f);
    }

    public static Vector2Int WorldToCell(Vector2 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
    }

    // Retângulos das ilhas (componentes conectados de células de ilha).
    public List<RectInt> FindIslands()
    {
        var islands = new List<RectInt>();
        var seen = new bool[width, height];
        var stack = new Stack<Vector2Int>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (seen[x, y] || !IsIsland(x, y)) continue;

                int minX = x, maxX = x, minY = y, maxY = y;
                stack.Push(new Vector2Int(x, y));
                seen[x, y] = true;
                while (stack.Count > 0)
                {
                    Vector2Int c = stack.Pop();
                    minX = Mathf.Min(minX, c.x); maxX = Mathf.Max(maxX, c.x);
                    minY = Mathf.Min(minY, c.y); maxY = Mathf.Max(maxY, c.y);
                    foreach (Vector2Int d in NavGrid.Cardinal)
                    {
                        Vector2Int n = c + d;
                        if (IsIsland(n.x, n.y) && !seen[n.x, n.y])
                        {
                            seen[n.x, n.y] = true;
                            stack.Push(n);
                        }
                    }
                }
                islands.Add(new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1));
            }
        }
        return islands;
    }

    // Rotas de tubarão: cada dígito de 1 a 9 marca os dois extremos de uma rota.
    public List<(Vector2Int a, Vector2Int b)> SharkRoutes()
    {
        var routes = new List<(Vector2Int, Vector2Int)>();
        for (char digit = '1'; digit <= '9'; digit++)
        {
            List<Vector2Int> ends = Find(digit);
            if (ends.Count == 2) routes.Add((ends[0], ends[1]));
        }
        return routes;
    }

    // Problemas que impedem o gerador de montar o mapa corretamente.
    public List<string> Validate()
    {
        var errors = new List<string>();
        if (Find(Start).Count != 1) errors.Add("O mapa precisa ter exatamente um 'S' (início).");
        if (Find(ChaserSpawn).Count == 0) errors.Add("O mapa precisa de pelo menos um 'P' (entrada de pirata).");

        foreach (RectInt r in FindIslands())
        {
            if (r.width < 2 || r.height < 2)
                errors.Add($"Ilha em ({r.x},{r.y}) precisa ter pelo menos 2x2.");
            for (int y = r.yMin; y < r.yMax; y++)
                for (int x = r.xMin; x < r.xMax; x++)
                    if (!IsIsland(x, y))
                    {
                        errors.Add($"Ilha em ({r.x},{r.y}) não é retangular.");
                        y = r.yMax;
                        break;
                    }
        }

        for (char digit = '1'; digit <= '9'; digit++)
        {
            int count = Find(digit).Count;
            if (count != 0 && count != 2) errors.Add($"A rota de tubarão '{digit}' precisa de exatamente 2 pontas (tem {count}).");
        }
        return errors;
    }
}
