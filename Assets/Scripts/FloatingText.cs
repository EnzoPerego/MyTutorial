using UnityEngine;
using TMPro;

// Texto que sobe e some no mundo ("+4s", "-5s", "Boia!").
public class FloatingText : MonoBehaviour
{
    public static readonly Color Gain = new Color(0.55f, 1f, 0.45f);
    public static readonly Color Loss = new Color(1f, 0.4f, 0.35f);
    public static readonly Color Info = new Color(0.55f, 0.9f, 1f);

    public static FloatingText prefab;

    public TextMeshPro text;
    public float duration = 0.9f;
    public float rise = 1.1f;

    private Vector3 start;
    private float t;

    public static void Spawn(Vector3 position, string message, Color color)
    {
        if (prefab == null) return;
        FloatingText ft = Instantiate(prefab, position, Quaternion.identity);
        ft.text.text = message;
        ft.text.color = color;
    }

    void Start()
    {
        start = transform.position;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = t / duration;
        transform.position = start + Vector3.up * rise * (1f - (1f - k) * (1f - k));
        transform.localScale = Vector3.one * (k < 0.15f ? Mathf.Lerp(0.6f, 1.2f, k / 0.15f) : Mathf.Lerp(1.2f, 1f, (k - 0.15f) * 4f));
        Color c = text.color;
        c.a = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
        text.color = c;
        if (k >= 1f) Destroy(gameObject);
    }
}
