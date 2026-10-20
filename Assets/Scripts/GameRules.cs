using UnityEngine;

// Números do jogo em um só lugar, para facilitar o balanceamento.
public static class GameRules
{
    public const float StartTime = 30f;

    public const float ChestBonus = 4f;
    public const float CoinBonus = 1f;
    public const float HourglassBonus = 5f;
    public const float WaveBonus = 5f;

    public const float ChaserPenalty = 5f;
    public const float CannonPenalty = 3f;
    public const float SharkPenalty = 3f;

    public const float InvincibleTime = 2f;

    public static int ChestsForWave(int wave)
    {
        return Mathf.Min(2 + wave, 8);
    }

    public static int CoinsForWave(int wave)
    {
        return Mathf.Min(4 + 2 * wave, 16);
    }

    public static int HourglassesForWave(int wave)
    {
        return wave >= 4 ? 2 : 1;
    }

    public static int BuoysForWave(int wave)
    {
        return wave % 2 == 0 ? 1 : 0;
    }

    public static int ChasersForWave(int wave)
    {
        if (wave >= 5) return 3;
        if (wave >= 3) return 2;
        return 1;
    }

    public static float ChaserSpeedMultiplier(int wave)
    {
        return Mathf.Min(1f + 0.05f * (wave - 1), 1.5f);
    }
}
