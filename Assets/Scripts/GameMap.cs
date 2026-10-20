using System.Collections.Generic;
using UnityEngine;

// Lê o layout da baía, mantém a grade de navegação e atualiza o campo de fluxo
// sempre que o surfista muda de célula.
public class GameMap : MonoBehaviour
{
    public static GameMap Instance { get; private set; }

    public TextAsset layoutFile;
    public LayerMask obstacleMask;
    public Transform player;

    public LevelLayout Layout { get; private set; }
    public NavGrid Nav { get; private set; }

    public Rect Bounds
    {
        get { return new Rect(0f, 0f, Layout.width, Layout.height); }
    }

    void Awake()
    {
        Instance = this;
        Layout = new LevelLayout(layoutFile.text);
        Nav = new NavGrid(Layout);
        Nav.BuildFlowField(Layout.FindStart());
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Update()
    {
        if (player != null) Nav.BuildFlowField(LevelLayout.WorldToCell(player.position));
    }

    // Células de água alcançáveis a partir do início, com água em volta (itens não grudam em ilhas).
    public List<Vector2Int> SpawnCandidates()
    {
        return SpawnCandidates(Layout, Nav);
    }

    public static List<Vector2Int> SpawnCandidates(LevelLayout layout, NavGrid nav)
    {
        var result = new List<Vector2Int>();
        foreach (Vector2Int c in nav.ReachableCells(layout.FindStart()))
        {
            char ch = layout[c.x, c.y];
            if (ch == LevelLayout.Start || ch == LevelLayout.ChaserSpawn) continue;

            bool open = true;
            for (int dy = -1; dy <= 1 && open; dy++)
                for (int dx = -1; dx <= 1 && open; dx++)
                    if (layout.IsBlocked(c.x + dx, c.y + dy)) open = false;
            if (open) result.Add(c);
        }
        return result;
    }
}
