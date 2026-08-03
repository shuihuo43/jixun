using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Room : MonoBehaviour
{
    [Header("出生点")]
    [SerializeField] private Transform playerBornPos;

    [Header("出口")]
    [SerializeField] private Collider2D exitCollider;

    [Header("Boss")]
    [SerializeField] private bool isBoss;
    [SerializeField] private Enemy bossEnemy;
    public bool IsBoss => isBoss;

    [Header("敌人生成")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int totalWaves = 3;
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private GameObject warningPrefab;

    public event Action OnExit;
    public bool isStartRoom;

    private List<Enemy> aliveEnemies = new();
    private HashSet<int> usedSpawnPoints = new();
    private int currentWave;
    private int enemiesSpawnedThisWave;

    void Start()
    {
        var rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (exitCollider != null)
        {
            exitCollider.isTrigger = true;
            // Boss 房间出口始终开着，条件由 OnTriggerEnter2D 判断
            exitCollider.enabled = isStartRoom || isBoss;
        }

        foreach (var tm in GetComponentsInChildren<Tilemap>())
            tm.RefreshAllTiles();
    }

    public void Init(Player player)
    {
        if (player != null && playerBornPos != null)
            player.transform.position = playerBornPos.position;

        if (!isBoss)
            StartWave();
    }

    void StartWave()
    {
        if (currentWave >= totalWaves) return;
        currentWave++;
        enemiesSpawnedThisWave = 0;
        aliveEnemies.Clear();
        usedSpawnPoints.Clear();
        SpawnNext();
    }

    void SpawnNext()
    {
        if (enemiesSpawnedThisWave >= enemiesPerWave) return;
        if (spawnPoints.Length == 0 || enemyPrefabs.Length == 0) return;

        if (usedSpawnPoints.Count >= spawnPoints.Length) usedSpawnPoints.Clear();
        int idx;
        do { idx = UnityEngine.Random.Range(0, spawnPoints.Length); }
        while (usedSpawnPoints.Contains(idx));
        usedSpawnPoints.Add(idx);
        var pos = spawnPoints[idx].position;

        // 预警
        if (warningPrefab != null)
        {
            var warn = Instantiate(warningPrefab, pos, Quaternion.identity);
            var entity = warn.GetComponent<Entity>();
            if (entity != null)
            {
                var root = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
                if (root == null) root = gameObject;
                entity.EntityBorn(root.transform.InverseTransformPoint(pos), Vector2.up, root, onDestroy: () => SpawnEnemy(pos));
            }
            else
            {
                SpawnEnemy(pos);
            }
        }
        else
        {
            SpawnEnemy(pos);
        }
    }

    void SpawnEnemy(Vector2 pos)
    {
        var prefab = enemyPrefabs[UnityEngine.Random.Range(0, enemyPrefabs.Length)];
        var obj = Instantiate(prefab, pos, Quaternion.identity);
        var enemy = obj.GetComponent<Enemy>();
        if (enemy != null)
        {
            aliveEnemies.Add(enemy);
            enemy.resource.OnDeath += () => OnEnemyDied(enemy);
        }

        enemiesSpawnedThisWave++;
        if (enemiesSpawnedThisWave < enemiesPerWave)
        {
            SpawnNext();
        }
        else
        {
            StartCoroutine(WaitForNextWave());
        }
    }

    System.Collections.IEnumerator WaitForNextWave()
    {
        int threshold = Mathf.Max(1, enemiesPerWave / 3);
        while (aliveEnemies.Count > threshold)
            yield return new WaitForSeconds(0.5f);

        if (currentWave >= totalWaves)
        {
            if (exitCollider != null) exitCollider.enabled = true;
        }
        else
            StartWave();
    }

    void OnEnemyDied(Enemy enemy)
    {
        aliveEnemies.Remove(enemy);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Room] OnTriggerEnter2D other={other.name} tag={other.tag} isBoss={isBoss}");

        if (!other.CompareTag("Player"))
        {
            Debug.Log($"[Room] 不是 Player，跳过");
            return;
        }

        // Boss 房间：Boss 死亡 + 场景无其他存活敌人才可离开
        if (isBoss)
        {
            bool bossAlive = bossEnemy != null && bossEnemy.resource != null && bossEnemy.resource.currentHealth > 0f;
            Debug.Log($"[Room] Boss 房间检测: bossEnemy={bossEnemy?.name}, bossAlive={bossAlive}");

            if (bossAlive) return;

            var all = FindObjectsOfType<Enemy>();
            foreach (var e in all)
            {
                bool alive = e.resource != null && e.resource.currentHealth > 0f
                          && e.stateMachine.CurrentStateName != "Death";
                Debug.Log($"[Room]   Enemy {e.name}: hp={e.resource?.currentHealth}, state={e.stateMachine.CurrentStateName}, alive={alive}");
                if (alive) return;
            }

            Debug.Log("[Room] Boss 房间所有敌人已死，允许离开");
        }

        OnExit?.Invoke();
        Debug.Log("[Room] OnExit 已触发");
    }
}



