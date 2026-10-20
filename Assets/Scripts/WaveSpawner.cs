using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Controla as ondas: espalha itens pelo mapa, coloca mais piratas e acelera os perseguidores.
public class WaveSpawner : MonoBehaviour
{
    public static WaveSpawner Instance { get; private set; }

    public GameMap map;
    public Transform player;
    public Pickup chestPrefab;
    public Pickup coinPrefab;
    public Pickup hourglassPrefab;
    public Pickup buoyPrefab;
    public EnemyChaser chaserPrefab;
    public GameObject spawnEffectPrefab;

    public float minDistanceFromPlayer = 6f;
    public float nextWaveDelay = 1.5f;
    public int maxLooseItems = 30;

    private readonly List<Pickup> chests = new List<Pickup>();
    private readonly List<Pickup> looseItems = new List<Pickup>();
    private readonly List<EnemyChaser> chasers = new List<EnemyChaser>();
    private readonly HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
    private List<Vector2Int> candidates;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable()
    {
        GameController.WaveCompleted += OnWaveCompleted;
        Pickup.Collected += OnCollected;
    }

    void OnDisable()
    {
        GameController.WaveCompleted -= OnWaveCompleted;
        Pickup.Collected -= OnCollected;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        candidates = map.SpawnCandidates();
        StartWave(1);
    }

    public IReadOnlyList<Pickup> Chests
    {
        get { return chests; }
    }

    public Pickup NearestChest(Vector2 from)
    {
        Pickup best = null;
        float bestDist = float.MaxValue;
        foreach (Pickup c in chests)
        {
            if (c == null) continue;
            float d = ((Vector2)c.transform.position - from).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = c;
            }
        }
        return best;
    }

    void StartWave(int wave)
    {
        int chestCount = GameRules.ChestsForWave(wave);
        GameController.StartWave(wave, chestCount);

        for (int i = 0; i < chestCount; i++) Spawn(chestPrefab, chests);
        SpawnLoose(hourglassPrefab, GameRules.HourglassesForWave(wave));
        SpawnLoose(buoyPrefab, GameRules.BuoysForWave(wave));
        SpawnLoose(coinPrefab, GameRules.CoinsForWave(wave));

        int before = chasers.Count;
        EnsureChasers(GameRules.ChasersForWave(wave), GameRules.ChaserSpeedMultiplier(wave));

        AudioManager.Play(Sfx.WaveStart);
        string subtitle = chasers.Count > before && wave > 1
            ? "Mais um navio pirata na baía!"
            : $"Pegue {chestCount} baús";
        if (WaveBanner.Instance != null) WaveBanner.Instance.Show($"Onda {wave}!", subtitle);
    }

    void SpawnLoose(Pickup prefab, int count)
    {
        for (int i = 0; i < count && looseItems.Count < maxLooseItems; i++) Spawn(prefab, looseItems);
    }

    void Spawn(Pickup prefab, List<Pickup> list)
    {
        if (!TryPickCell(out Vector2Int cell)) return;
        Pickup p = Instantiate(prefab, LevelLayout.CellCenter(cell), Quaternion.identity, transform);
        p.Cell = cell;
        occupied.Add(cell);
        list.Add(p);
    }

    bool TryPickCell(out Vector2Int cell)
    {
        Vector2 playerPos = player != null ? (Vector2)player.position : Vector2.zero;
        float minDist = minDistanceFromPlayer;
        for (int attempt = 0; attempt < 3; attempt++)
        {
            for (int i = 0; i < 60; i++)
            {
                Vector2Int c = candidates[Random.Range(0, candidates.Count)];
                if (occupied.Contains(c)) continue;
                if (Vector2.Distance(LevelLayout.CellCenter(c), playerPos) < minDist) continue;
                cell = c;
                return true;
            }
            minDist *= 0.5f;
        }
        cell = default;
        return false;
    }

    void OnCollected(Pickup p)
    {
        occupied.Remove(p.Cell);
        chests.Remove(p);
        looseItems.Remove(p);
    }

    void OnWaveCompleted()
    {
        GameTimer.AddTime(GameRules.WaveBonus);
        StartCoroutine(NextWave());
    }

    IEnumerator NextWave()
    {
        yield return new WaitForSeconds(nextWaveDelay);
        if (!GameController.gameOver) StartWave(GameController.wave + 1);
    }

    void EnsureChasers(int count, float speedMultiplier)
    {
        List<Vector2Int> spawns = map.Layout.Find(LevelLayout.ChaserSpawn);
        while (chasers.Count < count && spawns.Count > 0)
        {
            Vector2 pos = FarthestFromPlayer(spawns);
            EnemyChaser chaser = Instantiate(chaserPrefab, pos, Quaternion.identity);
            chasers.Add(chaser);
            if (spawnEffectPrefab != null) Instantiate(spawnEffectPrefab, pos, Quaternion.identity);
        }
        foreach (EnemyChaser c in chasers) c.speedMultiplier = speedMultiplier;
    }

    Vector2 FarthestFromPlayer(List<Vector2Int> cells)
    {
        Vector2 playerPos = player != null ? (Vector2)player.position : Vector2.zero;
        Vector2 best = LevelLayout.CellCenter(cells[0]);
        float bestDist = -1f;
        foreach (Vector2Int c in cells)
        {
            Vector2 p = LevelLayout.CellCenter(c);
            float d = Vector2.Distance(p, playerPos);
            foreach (EnemyChaser other in chasers)
                d = Mathf.Min(d, Vector2.Distance(p, other.transform.position) * 2f);
            if (d > bestDist)
            {
                bestDist = d;
                best = p;
            }
        }
        return best;
    }
}
