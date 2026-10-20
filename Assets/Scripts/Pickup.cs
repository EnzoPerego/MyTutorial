using System;
using UnityEngine;

public enum PickupType { Chest, Coin, Hourglass, Buoy }

// Item coletável. Balança na água e aplica o efeito quando o surfista encosta.
[RequireComponent(typeof(Collider2D))]
public class Pickup : MonoBehaviour
{
    public PickupType type;
    public Transform visual;
    public float bobHeight = 0.08f;
    public float bobSpeed = 2.5f;

    public Vector2Int Cell { get; set; }

    public static event Action<Pickup> Collected;

    private float seed;
    private float spawnTime;

    void Start()
    {
        seed = UnityEngine.Random.value * 10f;
        spawnTime = Time.time;
    }

    void Update()
    {
        if (visual == null) return;
        float t = Time.time + seed;
        float grow = Mathf.Clamp01((Time.time - spawnTime) / 0.3f);
        float pop = grow < 1f ? Mathf.Sin(grow * Mathf.PI * 0.5f) * 1.15f : 1f;
        visual.localPosition = new Vector3(0f, Mathf.Sin(t * bobSpeed) * bobHeight, 0f);
        visual.localScale = Vector3.one * pop * (1f + 0.04f * Mathf.Sin(t * bobSpeed * 2f));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null || GameController.gameOver) return;

        Collected?.Invoke(this);
        Apply(player);
        Destroy(gameObject);
    }

    void Apply(PlayerMovement player)
    {
        Vector3 textPos = transform.position + Vector3.up * 0.6f;
        switch (type)
        {
            case PickupType.Chest:
                AudioManager.Play(Sfx.Chest);
                GameTimer.AddTime(GameRules.ChestBonus);
                GameController.CollectChest();
                break;
            case PickupType.Coin:
                AudioManager.Play(Sfx.Coin, 0.7f);
                GameTimer.AddTime(GameRules.CoinBonus);
                break;
            case PickupType.Hourglass:
                AudioManager.Play(Sfx.PowerUp);
                GameTimer.AddTime(GameRules.HourglassBonus);
                break;
            case PickupType.Buoy:
                AudioManager.Play(Sfx.ShieldUp);
                player.GiveShield();
                FloatingText.Spawn(textPos, "Boia!", FloatingText.Info);
                break;
        }
    }
}
