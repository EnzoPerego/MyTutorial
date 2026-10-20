using UnityEngine;
using System.Collections;

// Surfista: movimento com inércia, remada forte (sprint com stamina), golpes, escudo e invencibilidade.
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float speed = 5.5f;
    public float acceleration = 10f;
    public float turnSpeed = 720f;
    public Transform visual;
    public GameObject shieldVisual;
    public SpriteRenderer[] blinkRenderers;

    public float invincibleTime = GameRules.InvincibleTime;
    public float blinkInterval = 0.1f;
    public float knockback = 6f;

    public float sprintMultiplier = 1.8f;
    public float drainPerSecond = 35f;
    public float regenPerSecond = 20f;
    public float minToSprint = 20f;
    public static float stamina;
    public static float maxStamina = 100f;

    private Rigidbody2D rb;
    private Vector2 input;
    private bool exhausted = false;
    private bool invincible = false;
    private bool hasShield = false;

    public bool HasShield
    {
        get { return hasShield; }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stamina = maxStamina;
        if (shieldVisual != null) shieldVisual.SetActive(false);
    }

    void Update()
    {
        if (Time.timeScale == 0f || GameController.gameOver)
        {
            input = Vector2.zero;
            return;
        }

        input = Vector2.ClampMagnitude(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")), 1f);

        if (visual != null && input.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg - 90f;
            Quaternion target = Quaternion.Euler(0f, 0f, angle);
            visual.rotation = Quaternion.RotateTowards(visual.rotation, target, turnSpeed * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        float currentSpeed = speed;
        bool wantsSprint = Input.GetButton("Fire3") && input.sqrMagnitude > 0.01f;

        if (exhausted && stamina >= minToSprint) exhausted = false;

        if (wantsSprint && !exhausted && stamina > 0f)
        {
            currentSpeed = speed * sprintMultiplier;
            stamina -= drainPerSecond * Time.fixedDeltaTime;
            if (stamina <= 0f)
            {
                stamina = 0f;
                exhausted = true;
            }
        }
        else
        {
            stamina = Mathf.Min(maxStamina, stamina + regenPerSecond * Time.fixedDeltaTime);
        }

        Vector2 desired = input * currentSpeed;
        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, desired, acceleration * Time.fixedDeltaTime);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        TryHazard(other.collider);
    }

    void OnCollisionStay2D(Collision2D other)
    {
        TryHazard(other.collider);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHazard(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryHazard(other);
    }

    void TryHazard(Collider2D other)
    {
        Hazard hazard = other.GetComponentInParent<Hazard>();
        if (hazard != null) TakeHit(hazard);
    }

    public bool TakeHit(Hazard hazard)
    {
        if (invincible || GameController.gameOver) return false;

        hazard.NotifyHit(this);
        Vector2 away = ((Vector2)transform.position - (Vector2)hazard.transform.position).normalized;
        rb.linearVelocity = away * knockback;

        if (hasShield)
        {
            hasShield = false;
            if (shieldVisual != null) shieldVisual.SetActive(false);
            AudioManager.Play(Sfx.ShieldBreak);
            FloatingText.Spawn(transform.position + Vector3.up, "Escudo!", FloatingText.Info);
            StartCoroutine(InvincibilityRoutine(1f));
            return true;
        }

        GameTimer.AddTime(-hazard.timePenalty);
        AudioManager.Play(Sfx.Hit);
        CameraFollow.Shake(0.35f, 0.3f);
        StartCoroutine(InvincibilityRoutine(invincibleTime));
        return true;
    }

    public void GiveShield()
    {
        hasShield = true;
        if (shieldVisual != null) shieldVisual.SetActive(true);
    }

    IEnumerator InvincibilityRoutine(float duration)
    {
        invincible = true;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            SetVisible(!blinkRenderers[0].enabled);
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        SetVisible(true);
        invincible = false;
    }

    void SetVisible(bool visible)
    {
        foreach (SpriteRenderer r in blinkRenderers) r.enabled = visible;
    }
}
