using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

// Roda o jogo de verdade por alguns segundos. Qualquer erro no console reprova o teste.
// Se a variável de ambiente FUGA_SHOTS apontar para uma pasta, salva screenshots do jogo rodando.
public class BaiaSmokeTests
{
    static IEnumerator LoadGame()
    {
        SceneManager.LoadScene(MenuAction.GameScene);
        yield return null;
        yield return null;
    }

    static Transform Player
    {
        get { return GameObject.FindGameObjectWithTag("Player").transform; }
    }

    [UnityTest]
    public IEnumerator FirstWaveSpawnsAndPirateChases()
    {
        yield return LoadGame();

        Assert.AreEqual(1, GameController.wave);
        Assert.AreEqual(GameRules.ChestsForWave(1), WaveSpawner.Instance.Chests.Count);
        var chaser = Object.FindFirstObjectByType<EnemyChaser>();
        Assert.IsNotNull(chaser, "nenhum navio pirata na onda 1");

        float before = Vector2.Distance(Player.position, chaser.transform.position);
        yield return new WaitForSeconds(2f);
        float after = Vector2.Distance(Player.position, chaser.transform.position);

        Assert.Less(after, before, "o pirata não se aproximou do surfista");
        Assert.Less(GameTimer.timeLeft, GameRules.StartTime);
        Assert.Greater(GameTimer.elapsedTime, 1.5f);
        Capture("jogo_rodando.png");
    }

    [UnityTest]
    public IEnumerator CollectingEveryChestStartsTheNextWave()
    {
        yield return LoadGame();
        var body = Player.GetComponent<Rigidbody2D>();

        for (int guard = 0; WaveSpawner.Instance.Chests.Count > 0; guard++)
        {
            Assert.Less(guard, 50, "os baús não estão sendo coletados");
            Pickup chest = WaveSpawner.Instance.Chests[0];
            body.position = chest.transform.position;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return null;
        }

        Capture("onda_completa.png");
        yield return new WaitForSeconds(2f);
        Assert.AreEqual(2, GameController.wave);
        Assert.AreEqual(GameRules.ChestsForWave(2), WaveSpawner.Instance.Chests.Count);
        Capture("onda_2.png");
    }

    [UnityTest]
    public IEnumerator RunningOutOfTimeEndsTheGameAndRestartWorks()
    {
        yield return LoadGame();
        float before = BestScore.BestTime;

        GameTimer.timeLeft = 0.05f;
        yield return new WaitForSeconds(0.3f);

        Assert.IsTrue(GameController.gameOver);
        var ui = Object.FindFirstObjectByType<UIManager>();
        Assert.IsTrue(ui.endGamePanel.activeSelf, "tela de fim de jogo não apareceu");
        Assert.IsFalse(ui.hud.activeSelf);
        Capture("fim_de_jogo.png");
        PlayerPrefs.SetFloat(BestScore.TimeKey, before);

        Object.FindFirstObjectByType<MenuAction>().IniciarJogo();
        yield return null;
        yield return null;
        Assert.IsFalse(GameController.gameOver);
        Assert.AreEqual(1, GameController.wave);
    }

    [UnityTest]
    public IEnumerator PauseFreezesTime()
    {
        yield return LoadGame();
        var pause = Object.FindFirstObjectByType<PauseMenu>();
        pause.SetPaused(true);
        yield return null;
        Assert.AreEqual(0f, Time.timeScale);
        Assert.IsTrue(pause.panel.activeSelf);
        Capture("pausa.png");
        pause.Continuar();
        Assert.AreEqual(1f, Time.timeScale);
    }

    static IEnumerator WaitPhysics(int steps)
    {
        for (int i = 0; i < steps; i++) yield return new WaitForFixedUpdate();
        yield return null;
    }

    [UnityTest]
    public IEnumerator SharkBiteCostsTime()
    {
        yield return LoadGame();
        var shark = Object.FindFirstObjectByType<SharkPatrol>();
        float before = GameTimer.timeLeft;
        Player.GetComponent<Rigidbody2D>().position = shark.transform.position;
        yield return WaitPhysics(3);
        Assert.Less(GameTimer.timeLeft, before - GameRules.SharkPenalty + 0.5f);
    }

    [UnityTest]
    public IEnumerator PirateContactCostsTimeAndPirateBacksOff()
    {
        yield return LoadGame();
        var chaser = Object.FindFirstObjectByType<EnemyChaser>();
        float before = GameTimer.timeLeft;
        chaser.GetComponent<Rigidbody2D>().position = (Vector2)Player.position + new Vector2(0.7f, 0f);
        yield return WaitPhysics(4);
        Assert.Less(GameTimer.timeLeft, before - GameRules.ChaserPenalty + 0.5f);
    }

    [UnityTest]
    public IEnumerator BuoyShieldAbsorbsOneHit()
    {
        yield return LoadGame();
        var player = Player.GetComponent<PlayerMovement>();
        player.GiveShield();
        var shark = Object.FindFirstObjectByType<SharkPatrol>();
        float before = GameTimer.timeLeft;
        Player.GetComponent<Rigidbody2D>().position = shark.transform.position;
        yield return WaitPhysics(3);
        Assert.IsFalse(player.HasShield, "a boia deveria ter sido gasta");
        Assert.Greater(GameTimer.timeLeft, before - 1f, "o escudo não bloqueou o golpe");
    }

    [UnityTest]
    public IEnumerator AnchoredShipFiresWhenPlayerIsInRange()
    {
        yield return LoadGame();
        CannonShooter ship = null;
        foreach (CannonShooter s in Object.FindObjectsByType<CannonShooter>(FindObjectsSortMode.None))
            if (!s.ignoreIslands) ship = s;
        Assert.IsNotNull(ship, "nenhum navio canhoneiro no mapa");

        Vector2 spot = Vector2.zero;
        foreach (Vector2 dir in new[] { Vector2.right, Vector2.left, Vector2.up, Vector2.down })
        {
            Vector2 candidate = (Vector2)ship.transform.position + dir * 3.5f;
            Vector2Int cell = LevelLayout.WorldToCell(candidate);
            if (GameMap.Instance.Nav.IsWalkable(cell))
            {
                spot = LevelLayout.CellCenter(cell);
                break;
            }
        }
        Assert.AreNotEqual(Vector2.zero, spot, "sem água perto do canhoneiro");

        var body = Player.GetComponent<Rigidbody2D>();
        bool fired = false;
        for (float t = 0f; t < ship.interval + 1.5f && !fired; t += Time.deltaTime)
        {
            body.position = spot;
            body.linearVelocity = Vector2.zero;
            fired = Object.FindFirstObjectByType<Cannonball>() != null;
            yield return null;
        }
        Assert.IsTrue(fired, "o canhoneiro não atirou com o surfista no alcance");
        Capture("canhao.png");
    }

    [UnityTest]
    public IEnumerator MenuHasPlayButton()
    {
        SceneManager.LoadScene(MenuAction.MenuScene);
        yield return null;
        yield return new WaitForSeconds(0.5f);
        Assert.IsNotNull(GameObject.Find("Botao Jogar")?.GetComponent<Button>());
        Capture("menu_rodando.png");
    }

    static void Capture(string file)
    {
        string dir = Environment.GetEnvironmentVariable("FUGA_SHOTS");
        if (string.IsNullOrEmpty(dir)) return;
        Directory.CreateDirectory(dir);

        Camera cam = Camera.main;
        var rt = new RenderTexture(1280, 720, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        tex.Apply();
        File.WriteAllBytes(Path.Combine(dir, file), tex.EncodeToPNG());
        RenderTexture.active = null;
        cam.targetTexture = null;
        Object.Destroy(rt);
        Object.Destroy(tex);
    }
}
