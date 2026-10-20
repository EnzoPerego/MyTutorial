using UnityEngine;

// Navio pirata perseguidor. Vai direto ao surfista quando enxerga; senão segue o campo de fluxo
// do GameMap, que contorna ilhas e pedras. Depois de acertar o surfista, fica parado um instante.
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyChaser : MonoBehaviour, IHazardListener
{
    public float speed = 3.2f;
    public float speedMultiplier = 1f;
    public float acceleration = 6f;
    public float turnSpeed = 200f;
    public float stunTime = 1f;
    public float sightRadius = 0.45f;
    public Transform visual;

    private Rigidbody2D rb;
    private Transform player;
    private float stunnedUntil;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void FixedUpdate()
    {
        if (GameController.gameOver || player == null || Time.time < stunnedUntil)
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, 8f * Time.fixedDeltaTime);
            return;
        }

        Vector2 target = DesiredDirection() * speed * speedMultiplier;
        rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, target, acceleration * Time.fixedDeltaTime);

        if (visual != null && rb.linearVelocity.sqrMagnitude > 0.05f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg - 90f;
            visual.rotation = Quaternion.RotateTowards(visual.rotation, Quaternion.Euler(0f, 0f, angle), turnSpeed * Time.fixedDeltaTime);
        }
    }

    Vector2 DesiredDirection()
    {
        Vector2 pos = rb.position;
        Vector2 toPlayer = (Vector2)player.position - pos;
        GameMap map = GameMap.Instance;
        if (map == null) return toPlayer.normalized;

        bool blocked = Physics2D.CircleCast(pos, sightRadius, toPlayer.normalized, toPlayer.magnitude, map.obstacleMask);
        if (!blocked) return toPlayer.normalized;

        if (map.Nav.TryGetNextCell(LevelLayout.WorldToCell(pos), out Vector2Int next))
            return (LevelLayout.CellCenter(next) - pos).normalized;

        return toPlayer.normalized;
    }

    public void OnHazardHit(PlayerMovement player)
    {
        stunnedUntil = Time.time + stunTime;
        Vector2 away = (rb.position - (Vector2)player.transform.position).normalized;
        rb.linearVelocity = away * 2.5f;
    }
}
