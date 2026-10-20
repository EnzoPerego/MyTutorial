using UnityEngine;

// Câmera que segue o surfista com suavização, presa às bordas do mapa, com tremida nos golpes.
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.15f;
    public Rect bounds;

    private static CameraFollow instance;
    private Camera cam;
    private Vector3 velocity;
    private float shakeAmount;
    private float shakeDuration;
    private float shakeTimer;

    void Awake()
    {
        instance = this;
        cam = GetComponent<Camera>();
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    public static void Shake(float amount, float duration)
    {
        if (instance == null) return;
        instance.shakeAmount = amount;
        instance.shakeDuration = duration;
        instance.shakeTimer = duration;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desired = Clamp(target.position);
        Vector3 pos = Vector3.SmoothDamp(transform.position - ShakeOffset(), desired, ref velocity, smoothTime);

        shakeTimer = Mathf.Max(0f, shakeTimer - Time.deltaTime);
        transform.position = pos + ShakeOffset();
    }

    Vector3 ShakeOffset()
    {
        if (shakeTimer <= 0f || shakeDuration <= 0f) return Vector3.zero;
        float strength = shakeAmount * (shakeTimer / shakeDuration);
        return new Vector3(Mathf.Sin(Time.time * 53f), Mathf.Cos(Time.time * 41f), 0f) * strength;
    }

    public Vector3 Clamp(Vector3 p)
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        float x = bounds.width > 2f * halfW ? Mathf.Clamp(p.x, bounds.xMin + halfW, bounds.xMax - halfW) : bounds.center.x;
        float y = bounds.height > 2f * halfH ? Mathf.Clamp(p.y, bounds.yMin + halfH, bounds.yMax - halfH) : bounds.center.y;
        return new Vector3(x, y, -10f);
    }
}
