using UnityEngine;
using TMPro;

// Mostra a onda atual, os baús coletados na onda e o recorde.
public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI waveText;
    public TextMeshProUGUI chestsText;
    public TextMeshProUGUI recordText;

    void Start()
    {
        if (recordText != null) recordText.text = GameTimer.Format(BestScore.BestTime, false);
    }

    void Update()
    {
        waveText.text = $"Onda {GameController.wave}";
        chestsText.text = $"{GameController.chestsCollected}/{GameController.chestsTotal}";
    }
}
