using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Pausa com ESC, P ou Start do controle.
public class PauseMenu : MonoBehaviour
{
    public GameObject panel;
    public Selectable firstSelected;

    public static bool paused { get; private set; }

    void Start()
    {
        SetPaused(false);
    }

    void OnDestroy()
    {
        paused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (GameController.gameOver) return;
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)
            || Input.GetKeyDown(KeyCode.JoystickButton7) || Input.GetKeyDown(KeyCode.JoystickButton9))
        {
            SetPaused(!paused);
        }
    }

    public void Continuar()
    {
        SetPaused(false);
    }

    public void SetPaused(bool value)
    {
        paused = value;
        panel.SetActive(value);
        Time.timeScale = value ? 0f : 1f;
        if (AudioManager.instance != null) AudioManager.instance.SetMusicPaused(value);
        if (value && firstSelected != null && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
    }
}
