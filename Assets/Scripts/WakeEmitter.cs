using System.Collections.Generic;
using UnityEngine;

// Rastro de espuma atrás de quem se move na água (surfista, navios, tubarões).
public class WakeEmitter : MonoBehaviour
{
    public Sprite foamSprite;
    public Material material;
    public Transform orientation;
    public Vector2 localOffset = new Vector2(0f, -0.5f);
    public float interval = 0.05f;
    public float life = 0.7f;
    public float minSpeed = 0.6f;
    public float startScale = 0.3f;
    public float endScale = 0.8f;
    public float alpha = 0.7f;
    public int sortingOrder = 2;
    public int poolSize = 24;

    private class Foam
    {
        public SpriteRenderer renderer;
        public float age;
    }

    private readonly List<Foam> pool = new List<Foam>();
    private Transform container;
    private Vector3 lastPosition;
    private float timer;
    private int next;

    void Start()
    {
        container = new GameObject(name + " (espuma)").transform;
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject("Espuma");
            go.transform.SetParent(container, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = foamSprite;
            if (material != null) sr.sharedMaterial = material;
            sr.sortingOrder = sortingOrder;
            sr.enabled = false;
            pool.Add(new Foam { renderer = sr, age = life });
        }
        lastPosition = transform.position;
    }

    void OnDestroy()
    {
        if (container != null) Destroy(container.gameObject);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        float speed = (transform.position - lastPosition).magnitude / dt;
        lastPosition = transform.position;

        timer -= dt;
        if (speed > minSpeed && timer <= 0f)
        {
            timer = interval;
            Emit();
        }

        foreach (Foam f in pool)
        {
            if (!f.renderer.enabled) continue;
            f.age += dt;
            float k = f.age / life;
            if (k >= 1f)
            {
                f.renderer.enabled = false;
                continue;
            }
            f.renderer.transform.localScale = Vector3.one * Mathf.Lerp(startScale, endScale, k);
            f.renderer.color = new Color(1f, 1f, 1f, alpha * (1f - k));
        }
    }

    void Emit()
    {
        Foam f = pool[next];
        next = (next + 1) % pool.Count;
        Transform basis = orientation != null ? orientation : transform;
        Vector2 jitter = Random.insideUnitCircle * 0.06f;
        f.renderer.transform.position = basis.TransformPoint(localOffset) + (Vector3)jitter;
        f.renderer.enabled = true;
        f.age = 0f;
    }
}
