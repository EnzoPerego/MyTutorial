using System;
using UnityEngine;
using TMPro;

// Relógio de vida (regressivo) e tempo sobrevivido (pontuação).
public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI survivedText;
    public float startTime = GameRules.StartTime;
    public float warningTime = 10f;
    public Color warningColor = new Color(1f, 0.35f, 0.3f);

    public static float elapsedTime;
    public static float timeLeft;

    public static event Action<float> TimeAdded;

    private Color normalColor;

    void Awake()
    {
        GameController.Init();
        elapsedTime = 0f;
        timeLeft = startTime;
        if (timerText != null) normalColor = timerText.color;
    }

    void Update()
    {
        if (!GameController.gameOver)
        {
            elapsedTime += Time.deltaTime;
            timeLeft -= Time.deltaTime;

            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                GameController.EndGame();
            }
        }

        if (timerText != null)
        {
            timerText.text = Format(timeLeft, true);
            bool warning = timeLeft <= warningTime && !GameController.gameOver;
            timerText.color = warning ? warningColor : normalColor;
            float pulse = warning ? 1f + 0.08f * Mathf.Abs(Mathf.Sin(Time.time * 6f)) : 1f;
            timerText.transform.localScale = Vector3.one * pulse;
        }
        if (survivedText != null) survivedText.text = Format(elapsedTime, false);
    }

    public static void AddTime(float seconds)
    {
        if (GameController.gameOver) return;
        timeLeft = Mathf.Max(0f, timeLeft + seconds);
        TimeAdded?.Invoke(seconds);
    }

    // mm:ss. O relógio de vida arredonda para cima, para não mostrar 00:00 com tempo sobrando.
    public static string Format(float seconds, bool roundUp)
    {
        int total = roundUp ? Mathf.CeilToInt(seconds) : Mathf.FloorToInt(seconds);
        total = Mathf.Max(0, total);
        return $"{total / 60:00}:{total % 60:00}";
    }
}
