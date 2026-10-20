using NUnit.Framework;
using UnityEngine;

public class GameRulesTests
{
    [TestCase(1, 3, 6, 1, 0, 1, 1.00f)]
    [TestCase(2, 4, 8, 1, 1, 1, 1.05f)]
    [TestCase(3, 5, 10, 1, 0, 2, 1.10f)]
    [TestCase(4, 6, 12, 2, 1, 2, 1.15f)]
    [TestCase(5, 7, 14, 2, 0, 3, 1.20f)]
    [TestCase(6, 8, 16, 2, 1, 3, 1.25f)]
    [TestCase(20, 8, 16, 2, 1, 3, 1.50f)]
    public void WaveTable(int wave, int chests, int coins, int hourglasses, int buoys, int chasers, float speed)
    {
        Assert.AreEqual(chests, GameRules.ChestsForWave(wave));
        Assert.AreEqual(coins, GameRules.CoinsForWave(wave));
        Assert.AreEqual(hourglasses, GameRules.HourglassesForWave(wave));
        Assert.AreEqual(buoys, GameRules.BuoysForWave(wave));
        Assert.AreEqual(chasers, GameRules.ChasersForWave(wave));
        Assert.AreEqual(speed, GameRules.ChaserSpeedMultiplier(wave), 0.0001f);
    }
}

public class GameControllerTests
{
    [Test]
    public void WaveCompletesWhenAllChestsAreCollected()
    {
        int completed = 0;
        void OnCompleted() => completed++;
        GameController.WaveCompleted += OnCompleted;
        try
        {
            GameController.Init();
            GameController.StartWave(1, 2);
            GameController.CollectChest();
            Assert.AreEqual(0, completed);
            GameController.CollectChest();
            Assert.AreEqual(1, completed);
        }
        finally
        {
            GameController.WaveCompleted -= OnCompleted;
        }
    }

    [Test]
    public void NothingCountsAfterGameOver()
    {
        GameController.Init();
        GameController.StartWave(1, 3);
        GameController.EndGame();
        GameController.CollectChest();
        Assert.AreEqual(0, GameController.chestsCollected);
        Assert.IsTrue(GameController.gameOver);
    }
}

public class TimeFormatTests
{
    [TestCase(65.2f, false, "01:05")]
    [TestCase(0.3f, true, "00:01")]
    [TestCase(0.3f, false, "00:00")]
    [TestCase(-2f, true, "00:00")]
    [TestCase(600f, false, "10:00")]
    public void Format(float seconds, bool roundUp, string expected)
    {
        Assert.AreEqual(expected, GameTimer.Format(seconds, roundUp));
    }
}

public class BestScoreTests
{
    float savedTime;
    int savedWave;
    bool hadTime;

    [SetUp]
    public void Save()
    {
        hadTime = PlayerPrefs.HasKey(BestScore.TimeKey);
        savedTime = BestScore.BestTime;
        savedWave = BestScore.BestWave;
        PlayerPrefs.DeleteKey(BestScore.TimeKey);
        PlayerPrefs.DeleteKey(BestScore.WaveKey);
    }

    [TearDown]
    public void Restore()
    {
        PlayerPrefs.DeleteKey(BestScore.TimeKey);
        PlayerPrefs.DeleteKey(BestScore.WaveKey);
        if (hadTime)
        {
            PlayerPrefs.SetFloat(BestScore.TimeKey, savedTime);
            PlayerPrefs.SetInt(BestScore.WaveKey, savedWave);
        }
        PlayerPrefs.Save();
    }

    [Test]
    public void KeepsOnlyTheLongestSurvival()
    {
        Assert.IsTrue(BestScore.Submit(10f, 2));
        Assert.IsFalse(BestScore.Submit(5f, 1));
        Assert.AreEqual(10f, BestScore.BestTime);
        Assert.AreEqual(2, BestScore.BestWave);
        Assert.IsTrue(BestScore.Submit(12.5f, 3));
        Assert.AreEqual(12.5f, BestScore.BestTime);
        Assert.AreEqual(3, BestScore.BestWave);
    }
}
