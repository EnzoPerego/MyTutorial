using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float startTime = 30f;

    public static float elapsedTime;
    public static float timeLeft;

    void Start()
    {
        GameController.Init();
        elapsedTime = 0f;
        timeLeft = startTime;
    }

    void Update()
    {
        if (GameController.gameOver) return;

        elapsedTime += Time.deltaTime;
        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            GameController.TimeUp();
        }

        int minutes = Mathf.FloorToInt(timeLeft / 60f);
        int seconds = Mathf.FloorToInt(timeLeft % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public static void AddTime(float seconds)
    {
        timeLeft += seconds;
    }
}