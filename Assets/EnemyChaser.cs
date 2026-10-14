using UnityEngine;

public class EnemyChaser : MonoBehaviour
{
    public float speed = 2f;

    private Rigidbody2D rb;
    private Transform player;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void FixedUpdate()
    {
        if (GameController.gameOver || player == null) return;

        Vector2 dir = ((Vector2)player.position - rb.position).normalized;
        rb.MovePosition(rb.position + dir * speed * Time.fixedDeltaTime);
    }
}