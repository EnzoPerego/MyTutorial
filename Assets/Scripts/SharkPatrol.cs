using UnityEngine;

// Tubarão que vai e volta entre dois pontos do recife.
[RequireComponent(typeof(Rigidbody2D))]
public class SharkPatrol : MonoBehaviour
{
    public Vector2 pointA;
    public Vector2 pointB;
    public float speed = 2.4f;
    public float turnSpeed = 300f;
    public float pause = 0.5f;
    public Transform visual;

    private Rigidbody2D rb;
    private bool goingToB = true;
    private float waitUntil;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.position = pointA;
    }

    void FixedUpdate()
    {
        Vector2 target = goingToB ? pointB : pointA;
        Vector2 pos = rb.position;

        if (Vector2.Distance(pos, target) < 0.05f)
        {
            goingToB = !goingToB;
            waitUntil = Time.time + pause;
            return;
        }

        if (visual != null)
        {
            Vector2 dir = target - pos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            visual.rotation = Quaternion.RotateTowards(visual.rotation, Quaternion.Euler(0f, 0f, angle), turnSpeed * Time.fixedDeltaTime);
            visual.localScale = new Vector3(1f + 0.04f * Mathf.Sin(Time.time * 7f), 1f, 1f);
        }

        if (Time.time < waitUntil) return;
        rb.MovePosition(Vector2.MoveTowards(pos, target, speed * Time.fixedDeltaTime));
    }
}
