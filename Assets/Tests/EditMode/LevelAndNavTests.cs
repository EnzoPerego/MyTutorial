using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class LevelLayoutTests
{
    [Test]
    public void FirstLineIsTheTopRow()
    {
        var layout = new LevelLayout("#.\n.S\n");
        Assert.AreEqual(2, layout.width);
        Assert.AreEqual(2, layout.height);
        Assert.AreEqual('#', layout[0, 1]);
        Assert.AreEqual('S', layout[1, 0]);
    }

    [Test]
    public void IslandsRocksAndAnchoredShipsBlockNavigation()
    {
        var layout = new LevelLayout("#pr\nCT.\nw.S\n");
        Assert.IsTrue(layout.IsBlocked(0, 2), "ilha");
        Assert.IsTrue(layout.IsBlocked(1, 2), "palmeira");
        Assert.IsTrue(layout.IsBlocked(2, 2), "pedra");
        Assert.IsTrue(layout.IsBlocked(0, 1), "navio canhoneiro");
        Assert.IsTrue(layout.IsBlocked(1, 1), "torre");
        Assert.IsTrue(layout.IsBlocked(0, 0), "muralha");
        Assert.IsFalse(layout.IsBlocked(2, 1), "água");
        Assert.IsFalse(layout.IsBlocked(2, 0), "início");
        Assert.IsTrue(layout.IsBlocked(-1, 0), "fora do mapa");
    }

    [Test]
    public void FindsIslandRectangles()
    {
        var layout = new LevelLayout("....\n.#p.\n.##.\n....\n");
        List<RectInt> islands = layout.FindIslands();
        Assert.AreEqual(1, islands.Count);
        Assert.AreEqual(new RectInt(1, 1, 2, 2), islands[0]);
    }

    [Test]
    public void ValidateRejectsNonRectangularIslands()
    {
        var layout = new LevelLayout("##.P\n#...\n...S\n");
        Assert.IsNotEmpty(layout.Validate());
    }

    [Test]
    public void ValidateRejectsSharkRouteWithOneEnd()
    {
        var layout = new LevelLayout("1..P\n...S\n");
        Assert.IsNotEmpty(layout.Validate());
    }
}

public class NavGridTests
{
    static Vector2Int[] Walk(NavGrid nav, Vector2Int from, int maxSteps)
    {
        var path = new List<Vector2Int> { from };
        Vector2Int current = from;
        for (int i = 0; i < maxSteps && current != nav.Target; i++)
        {
            Assert.IsTrue(nav.TryGetNextCell(current, out current), $"sem próximo passo em {current}");
            path.Add(current);
        }
        return path.ToArray();
    }

    [Test]
    public void FlowFieldGoesAroundAWall()
    {
        var layout = new LevelLayout(
            ".......\n" +
            "...#...\n" +
            "...#...\n" +
            "...#...\n" +
            "...#...\n");
        var nav = new NavGrid(layout);
        var target = new Vector2Int(6, 1);
        nav.BuildFlowField(target);

        Vector2Int[] path = Walk(nav, new Vector2Int(0, 1), 30);
        Assert.AreEqual(target, path[path.Length - 1]);
        foreach (Vector2Int c in path) Assert.IsTrue(nav.IsWalkable(c), $"passou por célula bloqueada {c}");
    }

    [Test]
    public void DoesNotCutIslandCorners()
    {
        var layout = new LevelLayout("#.\n..\n");
        var nav = new NavGrid(layout);
        nav.BuildFlowField(new Vector2Int(1, 1));
        Assert.IsTrue(nav.TryGetNextCell(new Vector2Int(0, 0), out Vector2Int next));
        Assert.AreEqual(new Vector2Int(1, 0), next);
    }

    [Test]
    public void ClosedPocketIsUnreachable()
    {
        var layout = new LevelLayout(
            ".....\n" +
            ".###.\n" +
            ".#.#.\n" +
            ".###.\n" +
            ".....\n");
        var nav = new NavGrid(layout);
        nav.BuildFlowField(new Vector2Int(0, 0));
        Assert.AreEqual(NavGrid.Unreachable, nav.DistanceTo(new Vector2Int(2, 2)));
        Assert.IsFalse(nav.TryGetNextCell(new Vector2Int(2, 2), out _));
    }

    [Test]
    public void ShipStuckOnIslandEdgeFindsWater()
    {
        var layout = new LevelLayout("....\n.##.\n....\n");
        var nav = new NavGrid(layout);
        nav.BuildFlowField(new Vector2Int(3, 0));
        Assert.IsTrue(nav.TryGetNextCell(new Vector2Int(2, 1), out Vector2Int next));
        Assert.IsTrue(nav.IsWalkable(next));
    }
}
