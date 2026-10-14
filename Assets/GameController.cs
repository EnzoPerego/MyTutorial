using UnityEngine;

public static class GameController
{
    private const int totalColectables = 7;
    private static int colectableCount = totalColectables;
    private static bool timeUp = false;

    public static bool gameOver
    {
        get { return colectableCount <= 0 || timeUp; }
    }

    public static bool won
    {
        get { return colectableCount <= 0; }
    }

    public static int collected
    {
        get { return totalColectables - colectableCount; }
    }

    public static int total
    {
        get { return totalColectables; }
    }

    public static void Init()
    {
        colectableCount = totalColectables;
        timeUp = false;
    }

    public static void Collect()
    {
        colectableCount--;
    }

    public static void TimeUp()
    {
        timeUp = true;
    }
}