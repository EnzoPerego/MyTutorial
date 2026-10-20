using UnityEngine;

// Seta que gira em volta do surfista apontando para o baú mais próximo.
public class Compass : MonoBehaviour
{
    public Transform player;
    public SpriteRenderer arrow;
    public float radius = 1.3f;
    public float fadeDistance = 3f;

    void LateUpdate()
    {
        Pickup chest = WaveSpawner.Instance != null ? WaveSpawner.Instance.NearestChest(player.position) : null;
        if (chest == null || GameController.gameOver)
        {
            arrow.enabled = false;
            return;
        }

        Vector2 toChest = chest.transform.position - player.position;
        Vector2 dir = toChest.normalized;
        transform.position = (Vector2)player.position + dir * radius;
        transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
        transform.localScale = Vector3.one * (1f + 0.1f * Mathf.Sin(Time.time * 5f));

        arrow.enabled = true;
        Color c = arrow.color;
        c.a = Mathf.Clamp01((toChest.magnitude - radius) / fadeDistance);
        arrow.color = c;
    }
}
