using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

// Esconde o HUD e mostra a tela de fim de jogo com tempo sobrevivido, onda e recorde.
public class UIManager : MonoBehaviour
{
    public GameObject hud;
    public GameObject endGamePanel;
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI recordText;
    public GameObject newRecordBadge;
    public Selectable firstSelected;

    void OnEnable()
    {
        GameController.GameEnded += OnGameEnded;
    }

    void OnDisable()
    {
        GameController.GameEnded -= OnGameEnded;
    }

    void Start()
    {
        endGamePanel.SetActive(false);
        hud.SetActive(true);
    }

    void OnGameEnded()
    {
        hud.SetActive(false);
        endGamePanel.SetActive(true);

        if (AudioManager.instance != null) AudioManager.instance.PlayEnd();

        float survived = GameTimer.elapsedTime;
        bool isRecord = BestScore.Submit(survived, GameController.wave);

        finalTimeText.text = $"Você sobreviveu {GameTimer.Format(survived, false)}";
        waveText.text = $"Onda alcançada: {GameController.wave}";
        recordText.text = $"Recorde: {GameTimer.Format(BestScore.BestTime, false)}";
        newRecordBadge.SetActive(isRecord);

        if (firstSelected != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
    }
}
