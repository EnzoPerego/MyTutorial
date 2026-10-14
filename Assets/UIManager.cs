using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject endGamePanel;
    public TextMeshProUGUI finalTimeText;
    public GameObject timerText;
    public GameObject[] hudElements;

    void Start()
    {
        endGamePanel.SetActive(false);
        timerText.SetActive(true);
        foreach (GameObject g in hudElements)
        {
            g.SetActive(true);
        }
    }

    void Update()
    {
        if (GameController.gameOver && !endGamePanel.activeSelf)
        {
            endGamePanel.SetActive(true);
            timerText.SetActive(false);
            foreach (GameObject g in hudElements)
            {
                g.SetActive(false);
            }

            if (AudioManager.instance != null) AudioManager.instance.PlayEnd(GameController.won);

            float t = GameTimer.timeLeft;
            int minutes = Mathf.FloorToInt(t / 60f);
            int seconds = Mathf.FloorToInt(t % 60f);
            string tempo = $"{minutes:00}:{seconds:00}";

            if (GameController.won)
            {
                finalTimeText.text = $"Você venceu! Sobraram {tempo}";
            }
            else
            {
                finalTimeText.text = $"Tempo esgotado! ({tempo})";
            }
        }
    }
}