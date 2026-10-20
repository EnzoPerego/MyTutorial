using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

// Verifica o mapa real da baía: nada de áreas presas (softlock) e tubarões só na água.
public class BaiaMapTests
{
    LevelLayout layout;
    NavGrid nav;

    [SetUp]
    public void Load()
    {
        layout = new LevelLayout(File.ReadAllText("Assets/Level/baia.txt"));
        nav = new NavGrid(layout);
    }

    [Test]
    public void LayoutIsValid()
    {
        CollectionAssert.IsEmpty(layout.Validate());
    }

    [Test]
    public void EveryWaterCellIsReachableFromStart()
    {
        var reachable = new HashSet<Vector2Int>(nav.ReachableCells(layout.FindStart()));
        var stuck = new List<Vector2Int>();
        for (int y = 0; y < layout.height; y++)
            for (int x = 0; x < layout.width; x++)
                if (!layout.IsBlocked(x, y) && !reachable.Contains(new Vector2Int(x, y)))
                    stuck.Add(new Vector2Int(x, y));
        CollectionAssert.IsEmpty(stuck, "Células de água presas: " + string.Join(" ", stuck));
    }

    [Test]
    public void ChaserSpawnsAreReachable()
    {
        var reachable = new HashSet<Vector2Int>(nav.ReachableCells(layout.FindStart()));
        foreach (Vector2Int p in layout.Find(LevelLayout.ChaserSpawn))
            Assert.IsTrue(reachable.Contains(p), $"Entrada de pirata presa em {p}");
    }

    [Test]
    public void SharkRoutesStayOnWater()
    {
        const float sharkRadius = 0.4f;
        foreach ((Vector2Int a, Vector2Int b) in layout.SharkRoutes())
        {
            Vector2 pa = LevelLayout.CellCenter(a), pb = LevelLayout.CellCenter(b);
            Vector2 side = Vector2.Perpendicular((pb - pa).normalized) * sharkRadius;
            for (int i = 0; i <= 60; i++)
            {
                Vector2 p = Vector2.Lerp(pa, pb, i / 60f);
                foreach (Vector2 q in new[] { p, p + side, p - side })
                {
                    Vector2Int cell = LevelLayout.WorldToCell(q);
                    Assert.IsFalse(layout.IsBlocked(cell.x, cell.y), $"Rota de tubarão {a}->{b} passa por {cell}");
                }
            }
        }
    }

    [Test]
    public void HasRoomForAWaveFullOfItems()
    {
        int maxItems = GameRules.ChestsForWave(100) + 30;
        Assert.GreaterOrEqual(GameMap.SpawnCandidates(layout, nav).Count, maxItems * 2);
    }
}
