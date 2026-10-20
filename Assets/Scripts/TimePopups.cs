using UnityEngine;

// Mostra "+4s" / "-5s" sobre o surfista sempre que o relógio muda por um item ou golpe.
public class TimePopups : MonoBehaviour
{
    public FloatingText textPrefab;
    public Transform player;

    void Awake()
    {
        FloatingText.prefab = textPrefab;
    }

    void OnEnable()
    {
        GameTimer.TimeAdded += OnTimeAdded;
    }

    void OnDisable()
    {
        GameTimer.TimeAdded -= OnTimeAdded;
    }

    void OnTimeAdded(float seconds)
    {
        if (player == null) return;
        Vector3 pos = player.position + new Vector3(Random.Range(-0.3f, 0.3f), 0.8f, 0f);
        string message = seconds > 0f ? $"+{seconds:0}s" : $"-{-seconds:0}s";
        FloatingText.Spawn(pos, message, seconds > 0f ? FloatingText.Gain : FloatingText.Loss);
    }
}
