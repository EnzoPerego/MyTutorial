using UnityEngine;

// No menu, a câmera passeia devagar pela baía.
[RequireComponent(typeof(Camera))]
public class MenuCameraPan : MonoBehaviour
{
    public Rect bounds;
    public float speed = 0.05f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float rx = Mathf.Max(0f, bounds.width * 0.5f - halfW);
        float ry = Mathf.Max(0f, bounds.height * 0.5f - halfH);
        float t = Time.time * speed;
        Vector2 c = bounds.center;
        transform.position = new Vector3(c.x + Mathf.Sin(t * 2f * Mathf.PI) * rx, c.y + Mathf.Sin(t * 3f * Mathf.PI) * ry, -10f);
    }
}
