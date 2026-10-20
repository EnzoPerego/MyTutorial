using System;

// Estado da partida: onda atual, baús da onda e fim de jogo.
public static class GameController
{
    public static int wave { get; private set; }
    public static int chestsTotal { get; private set; }
    public static int chestsCollected { get; private set; }
    public static bool gameOver { get; private set; }

    public static event Action WaveCompleted;
    public static event Action GameEnded;

    public static void Init()
    {
        wave = 0;
        chestsTotal = 0;
        chestsCollected = 0;
        gameOver = false;
    }

    public static void StartWave(int number, int chests)
    {
        wave = number;
        chestsTotal = chests;
        chestsCollected = 0;
    }

    public static void CollectChest()
    {
        if (gameOver) return;
        chestsCollected++;
        if (chestsCollected == chestsTotal) WaveCompleted?.Invoke();
    }

    public static void EndGame()
    {
        if (gameOver) return;
        gameOver = true;
        GameEnded?.Invoke();
    }
}
