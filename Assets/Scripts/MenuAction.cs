using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuAction : MonoBehaviour
{
  public const string GameScene = "Baia";
  public const string MenuScene = "MainMenu";

  public GameObject howToPanel;
  public Selectable mainFirst;
  public Selectable howToFirst;
  public TextMeshProUGUI recordText;

  void Start()
  {
    if (howToPanel != null) howToPanel.SetActive(false);
    if (recordText != null)
    {
      recordText.text = BestScore.BestTime > 0f
        ? $"Recorde: {GameTimer.Format(BestScore.BestTime, false)} (onda {BestScore.BestWave})"
        : "Sem recorde ainda. Quanto tempo você aguenta?";
    }
  }

  public void IniciarJogo()
  {
    Time.timeScale = 1f;
    SceneManager.LoadScene(GameScene);
  }

  public void MenuPrincipal()
  {
    Time.timeScale = 1f;
    SceneManager.LoadScene(MenuScene);
  }

  public void AbrirComoJogar()
  {
    howToPanel.SetActive(true);
    Select(howToFirst);
  }

  public void FecharComoJogar()
  {
    howToPanel.SetActive(false);
    Select(mainFirst);
  }

  public void Clique()
  {
    AudioManager.Play(Sfx.Click);
  }

  static void Select(Selectable s)
  {
    if (s != null && EventSystem.current != null) EventSystem.current.SetSelectedGameObject(s.gameObject);
  }
}
