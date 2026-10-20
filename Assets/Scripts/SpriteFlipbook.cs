using UnityEngine;

// Efeito curto: troca quadros, cresce e some (explosão, respingo, fumaça do canhão).
public class SpriteFlipbook : MonoBehaviour
{
    public SpriteRenderer target;
    public Sprite[] frames;
    public float duration = 0.45f;
    public float startScale = 0.5f;
    public float endScale = 1f;
    public bool fade = true;

    private float t;
    private Color baseColor;

    void Start()
    {
        baseColor = target.color;
        transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / duration);
        if (frames != null && frames.Length > 0)
            target.sprite = frames[Mathf.Min(frames.Length - 1, (int)(k * frames.Length))];
        transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, k);
        if (fade)
        {
            Color c = baseColor;
            c.a = baseColor.a * (k < 0.5f ? 1f : 1f - (k - 0.5f) * 2f);
            target.color = c;
        }
        if (k >= 1f) Destroy(gameObject);
    }
}
