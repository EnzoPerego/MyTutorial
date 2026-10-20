using UnityEngine;
using TMPro;

// Aviso grande no meio da tela no começo de cada onda.
public class WaveBanner : MonoBehaviour
{
    public static WaveBanner Instance { get; private set; }

    public CanvasGroup group;
    public TextMeshProUGUI title;
    public TextMeshProUGUI subtitle;
    public float holdTime = 1.6f;
    public float fadeTime = 0.4f;

    private float shownAt = -100f;

    void Awake()
    {
        Instance = this;
        group.alpha = 0f;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Show(string titleText, string subtitleText)
    {
        title.text = titleText;
        subtitle.text = subtitleText;
        shownAt = Time.time;
    }

    void Update()
    {
        float t = Time.time - shownAt;
        if (t > holdTime + fadeTime)
        {
            group.alpha = 0f;
            return;
        }
        group.alpha = t < holdTime ? 1f : 1f - (t - holdTime) / fadeTime;
        float pop = t < 0.2f ? Mathf.Lerp(1.4f, 1f, t / 0.2f) : 1f;
        transform.localScale = Vector3.one * pop;
    }
}
