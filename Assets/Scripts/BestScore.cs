using UnityEngine;

// Recorde = maior tempo sobrevivido. Fica salvo no navegador (PlayerPrefs) entre partidas.
public static class BestScore
{
    public const string TimeKey = "FugaNaBaia.MelhorTempo";
    public const string WaveKey = "FugaNaBaia.MelhorOnda";

    public static float BestTime
    {
        get { return PlayerPrefs.GetFloat(TimeKey, 0f); }
    }

    public static int BestWave
    {
        get { return PlayerPrefs.GetInt(WaveKey, 0); }
    }

    // Retorna true se for um novo recorde.
    public static bool Submit(float survivedTime, int wave)
    {
        if (survivedTime <= BestTime) return false;
        PlayerPrefs.SetFloat(TimeKey, survivedTime);
        PlayerPrefs.SetInt(WaveKey, wave);
        PlayerPrefs.Save();
        return true;
    }
}
