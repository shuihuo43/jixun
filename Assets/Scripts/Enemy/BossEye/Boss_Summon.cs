using UnityEngine;

public class Boss_Summon : EnemyState
{
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private int spawnCount = 3;
    [SerializeField] private float spawnRadius = 5f;

    private float timer;
    private int spawned;
    private Boss_Eye boss;

    public override void StateEnter()
    {
        boss = enemy as Boss_Eye;
        timer = 0f;
        spawned = 0;
        enemy.moveDir = Vector2.zero;
        boss.lastSummonTime = Time.time;
    }

    public override void StateUpdate()
    {
        if (spawned >= spawnCount)
        {
            stateMachine.ChangeToState(enemy.EngageStateName);
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = spawnInterval;
            TrySpawnWarning();
        }
    }

    void TrySpawnWarning()
    {
        if (boss == null) return;

        Vector2 spawnPos;
        int attempt = 0;
        do
        {
            Vector2 offset = Random.insideUnitCircle * spawnRadius;
            spawnPos = (Vector2)enemy.transform.position + offset;
            Vector2 dir = spawnPos - (Vector2)enemy.transform.position;
            if (!Physics2D.Raycast(enemy.transform.position, dir.normalized, dir.magnitude, LayerMask.GetMask("Wall")))
                break;
        } while (++attempt < 10);

        var obj = Instantiate(boss.summonWarningPrefab, spawnPos, Quaternion.identity);
        var entity = obj.GetComponent<Entity>();
        if (entity != null)
        {
            var root = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
            if (root == null) root = enemy.gameObject;
            var localPos = root.transform.InverseTransformPoint(spawnPos);
            entity.EntityBorn(localPos, Vector2.up, root, onDestroy: () => SpawnMinion(spawnPos));
        }

        spawned++;
    }

    void SpawnMinion(Vector2 pos)
    {
        if (boss?.summonEnemyPrefab == null) return;

        var obj = Instantiate(boss.summonEnemyPrefab, pos, Quaternion.identity);
        var minion = obj.GetComponent<EyeMinion>();
        if (minion != null)
        {
            minion.eyeBoss = boss;
            boss.activeMinionCount++;
            minion.resource.OnDeath += () => boss.activeMinionCount--;
        }
    }
}
