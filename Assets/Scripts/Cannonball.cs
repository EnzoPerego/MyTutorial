using UnityEngine;

// Bala de canhão: explode ao bater em ilha, pedra ou no surfista; cai na água ao fim do alcance.
[RequireComponent(typeof(Rigidbody2D))]
public class Cannonball : MonoBehaviour, IHazardListener
{
    public float lifetime = 1.4f;
    public GameObject explosionPrefab;
    public GameObject splashPrefab;

    private Rigidbody2D rb;
    private LayerMask blockMask;
    private float dieAt;
    private bool done;

    public void Launch(Vector2 velocity, LayerMask mask)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = velocity;
        blockMask = mask;
        dieAt = Time.time + lifetime;
    }

    void Update()
    {
        if (Time.time >= dieAt) Finish(splashPrefab, Sfx.Splash);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & blockMask) != 0) Finish(explosionPrefab, Sfx.Explosion);
    }

    public void OnHazardHit(PlayerMovement player)
    {
        Finish(explosionPrefab, Sfx.Explosion);
    }

    void Finish(GameObject effect, Sfx sound)
    {
        if (done) return;
        done = true;
        if (effect != null) Instantiate(effect, transform.position, Quaternion.identity);
        AudioManager.PlayAt(sound, transform.position);
        Destroy(gameObject);
    }
}
