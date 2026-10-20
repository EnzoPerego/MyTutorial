using UnityEngine;

// Canhão de navio ancorado ou de torre. Mira no surfista quando ele está no alcance e à vista,
// fica vermelho piscando antes do tiro e dispara uma bala.
public class CannonShooter : MonoBehaviour
{
    public Cannonball ballPrefab;
    public GameObject muzzlePrefab;
    public Transform barrel;
    public SpriteRenderer barrelRenderer;
    public float range = 7f;
    public float interval = 2.2f;
    public float windup = 0.6f;
    public float ballSpeed = 7f;
    public bool ignoreIslands = false;
    public Color warningColor = new Color(1f, 0.35f, 0.3f);

    private Transform player;
    private float nextShot;
    private Color barrelColor = Color.white;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        if (barrelRenderer != null) barrelColor = barrelRenderer.color;
        nextShot = Time.time + Random.Range(windup, interval);
    }

    LayerMask BlockMask
    {
        get
        {
            if (GameMap.Instance == null) return 0;
            LayerMask mask = GameMap.Instance.obstacleMask;
            if (ignoreIslands) mask &= ~LayerMask.GetMask("Island");
            return mask;
        }
    }

    void Update()
    {
        if (player == null || GameController.gameOver)
        {
            SetWarning(0f);
            return;
        }

        Vector2 origin = barrel.position;
        Vector2 toPlayer = (Vector2)player.position - origin;
        float angle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
        barrel.rotation = Quaternion.RotateTowards(barrel.rotation, Quaternion.Euler(0f, 0f, angle), 270f * Time.deltaTime);

        bool inSight = toPlayer.magnitude <= range
            && !Physics2D.Raycast(origin, toPlayer.normalized, toPlayer.magnitude, BlockMask);

        if (!inSight)
        {
            nextShot = Mathf.Max(nextShot, Time.time + windup);
            SetWarning(0f);
            return;
        }

        float remaining = nextShot - Time.time;
        SetWarning(remaining < windup ? Mathf.PingPong(Time.time * 8f, 1f) : 0f);

        if (remaining <= 0f)
        {
            Fire(toPlayer.normalized);
            nextShot = Time.time + interval;
        }
    }

    void Fire(Vector2 direction)
    {
        Vector2 muzzle = (Vector2)barrel.position + direction * 0.45f;
        Cannonball ball = Instantiate(ballPrefab, muzzle, Quaternion.identity);
        ball.Launch(direction * ballSpeed, BlockMask);
        if (muzzlePrefab != null) Instantiate(muzzlePrefab, muzzle, Quaternion.identity);
        AudioManager.PlayAt(Sfx.Cannon, muzzle);
    }

    void SetWarning(float amount)
    {
        if (barrelRenderer != null) barrelRenderer.color = Color.Lerp(barrelColor, warningColor, amount);
    }
}
