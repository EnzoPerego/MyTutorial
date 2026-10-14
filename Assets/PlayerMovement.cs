using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    public float speed;
    AudioSource audioSource;

    public float invincibleTime = 2f;
    public float blinkInterval = 0.1f;
    private bool invincible = false;
    private SpriteRenderer sr;

    public float sprintMultiplier = 1.8f;
    public float drainPerSecond = 35f;
    public float regenPerSecond = 20f;
    public float minToSprint = 20f;
    public static float stamina;
    public static float maxStamina = 100f;
    private bool exhausted = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        sr = GetComponent<SpriteRenderer>();
        stamina = maxStamina;
    }

    void FixedUpdate()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector2 movement = new Vector2(moveHorizontal, moveVertical);

        float currentSpeed = speed;
        bool wantsSprint = Input.GetButton("Fire3") && movement.sqrMagnitude > 0.01f;

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

        rb.MovePosition(rb.position + movement.normalized * currentSpeed * Time.fixedDeltaTime);
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.tag == "Coletavel")
        {
            audioSource.Play();
            GameController.Collect();
            GameTimer.AddTime(1f);
            Destroy(other.gameObject);
        }
        else if (other.gameObject.tag == "Inimigo")
        {
            TakeHit();
        }
    }

    void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.tag == "Inimigo")
        {
            TakeHit();
        }
    }

    void TakeHit()
    {
        if (invincible || GameController.gameOver) return;

        GameTimer.AddTime(-5f);
        if (AudioManager.instance != null) AudioManager.instance.PlayHit();
        StartCoroutine(InvincibilityRoutine());
    }

    IEnumerator InvincibilityRoutine()
    {
        invincible = true;

        float elapsed = 0f;
        while (elapsed < invincibleTime)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        sr.enabled = true;
        invincible = false;
    }
}