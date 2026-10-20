using UnityEngine;

// Anima as cristas de onda decorativas: aparecem e somem devagar e balançam de lado.
public class OceanDecor : MonoBehaviour
{
    public float minAlpha = 0.05f;
    public float maxAlpha = 0.55f;
    public float speed = 0.6f;
    public float drift = 0.15f;

    private SpriteRenderer[] waves;
    private Vector3[] origins;
    private float[] phases;

    void Start()
    {
        waves = GetComponentsInChildren<SpriteRenderer>();
        origins = new Vector3[waves.Length];
        phases = new float[waves.Length];
        for (int i = 0; i < waves.Length; i++)
        {
            origins[i] = waves[i].transform.localPosition;
            phases[i] = Random.value * Mathf.PI * 2f;
        }
    }

    void Update()
    {
        float t = Time.time * speed;
        for (int i = 0; i < waves.Length; i++)
        {
            float s = Mathf.Sin(t + phases[i]);
            Color c = waves[i].color;
            c.a = Mathf.Lerp(minAlpha, maxAlpha, 0.5f + 0.5f * s);
            waves[i].color = c;
            waves[i].transform.localPosition = origins[i] + new Vector3(Mathf.Cos(t * 0.7f + phases[i]) * drift, 0f, 0f);
        }
    }
}
