using UnityEngine;

/// <summary>Boss 眼球立回：距离保持 + 前后波动 + 平行跟随拦截玩家</summary>
public class Eye_Follow : EnemyState
{
    [Header("距离")]
    /// <summary>期望保持的距离</summary>
    [SerializeField] private float preferredDistance = 4f;
    /// <summary>最大移动速度</summary>
    [SerializeField] private float maxSpeed = 5f;

    [Header("波动")]
    /// <summary>前后浮动幅度</summary>
    [SerializeField] private float wobbleAmp = 0.5f;
    /// <summary>前后浮动频率</summary>
    [SerializeField] private float wobbleFreq = 1.2f;

    [Header("平行拦截")]
    /// <summary>跟随玩家移动方向的程度</summary>
    [SerializeField] private float followWeight = 1.2f;
    /// <summary>向玩家移动路径靠拢的程度</summary>
    [SerializeField] private float interceptWeight = 0.8f;

    [Header("冲刺")]
    /// <summary>游走时间最小值</summary>
    [SerializeField] private float wanderTimeMin = 1.5f;
    /// <summary>游走时间最大值</summary>
    [SerializeField] private float wanderTimeMax = 3f;

    [Header("大范围游走")]
    /// <summary>游走范围</summary>
    [SerializeField] private float patrolRadius = 10f;
    /// <summary>游走速度</summary>
    [SerializeField] private float patrolSpeed = 5f;
    /// <summary>目标切换距离</summary>
    [SerializeField] private float minPatrolDist = 5f;

    [Header("切换")]
    [SerializeField] private string[] stateArray = { "DashAttack", "Summon", "Throw" };

    [Header("射击")]
    /// <summary>子弹预制体</summary>
    [SerializeField] private GameObject bulletPrefab;
    /// <summary>发射间隔（秒）</summary>
    [SerializeField] private float fireInterval = 1.5f;
    /// <summary>每次发射数量</summary>
    [SerializeField] private int bulletCount = 3;
    /// <summary>散射角度</summary>
    [SerializeField] private float spreadAngle = 20f;
    /// <summary>生成半径（离中心距离）</summary>
    [SerializeField] private float spawnRadius = 1f;

    private float wobbleOffset;
    private float fireTimer;
    private float wanderTimer;
    private Vector2 smoothVel;
    private Vector2 patrolTarget;

    public override void StateEnter()
    {
        wobbleOffset = Random.Range(0f, Mathf.PI * 2f);
        wanderTimer = Random.Range(wanderTimeMin, wanderTimeMax);
    }

    public override void StateUpdate()
    {
        if (enemy.Player == null) return;

        Vector2 toPlayer = enemy.Player.position - enemy.transform.position;
        float dist = toPlayer.magnitude;
        Vector2 dir = dist > 0.01f ? toPlayer / dist : Vector2.right;

        // 始终面向玩家
        enemy.faceDir = dir;

        //// 1. 距离驱动力（越远越快靠近）
        //float distForce = Mathf.Clamp((dist - preferredDistance) / preferredDistance, -1f, 1f);
        //Vector2 distMove = dir * distForce * maxSpeed;
        //
        //// 2. 前后正弦波动
        //float wobble = Mathf.Sin(Time.time * wobbleFreq + wobbleOffset) * wobbleAmp;
        //Vector2 wobbleMove = dir * wobble;
        //
        //// 3. 平行跟随 + 拦截
        //Vector2 interceptMove = Vector2.zero;
        //var player = enemy.Player.GetComponent<Player>();
        //if (player != null && player.MoveInput.sqrMagnitude > 0.01f)
        //{
        //    Vector2 followDir = player.MoveInput.normalized;
        //    interceptMove += followDir * followWeight;
        //    Vector2 playerVel = player.MoveInput * player.MoveSpeed;
        //    Vector2 ahead = (Vector2)enemy.Player.position + playerVel * 0.3f;
        //    Vector2 toAhead = ahead - (Vector2)enemy.transform.position;
        //    interceptMove += toAhead.normalized * interceptWeight;
        //}

        // 趋向场中心游走 + 随机偏移
        Vector2 toCenter = -(Vector2)enemy.transform.position;
        Vector2 wanderOffset = Random.insideUnitCircle * patrolRadius * 0.5f;
        Vector2 target = toCenter + wanderOffset;
        Vector2 patrolDir = target.normalized;
        smoothVel = Vector2.Lerp(smoothVel, patrolDir * patrolSpeed, 3f * Time.deltaTime);
        enemy.moveDir = smoothVel.sqrMagnitude > 0.01f ? smoothVel.normalized : Vector2.zero;

        // 4. 切换状态
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f && stateArray.Length > 0)
        {
            string pick = stateArray[Random.Range(0, stateArray.Length)];
            if (pick == "Summon")
            {
                var boss = enemy as Boss_Eye;
                if (boss != null)
                {
                    float sinceLast = Time.time - boss.lastSummonTime;
                    bool forced = sinceLast >= boss.forceSummonInterval;
                    bool normal = sinceLast >= boss.summonInterval && boss.activeMinionCount < 2;
                    if (!forced && !normal) pick = "DashAttack";
                }
            }
            stateMachine.ChangeToState(pick);
            return;
        }

        //// 5. 射击
        //fireTimer -= Time.deltaTime;
        //if (fireTimer <= 0f)
        //{
        //    Debug.Log($"[Eye_Follow] Firing, bulletPrefab={bulletPrefab?.name}, count={bulletCount}");
        //    fireTimer = fireInterval;
        //    FireBullets(dir);
        //}
    }

    void FireBullets(Vector2 aimDir)
    {
        if (bulletPrefab == null) { Debug.LogWarning("[Eye_Follow] bulletPrefab is null"); return; }

        GameObject root = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
        if (root == null) root = enemy.gameObject;

        float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        float halfSpan = spreadAngle * (bulletCount - 1) / 2f;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = -halfSpan + spreadAngle * i + baseAngle;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            Vector2 spawnPos = (Vector2)enemy.transform.position + dir * spawnRadius;
            var obj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);
            var bullet = obj.GetComponent<BulletEntity>();
            if (bullet != null)
            {
                Vector2 localPos = root.transform.InverseTransformPoint(spawnPos);
                bullet.EntityBorn(localPos, dir, root, ownerObj: enemy.gameObject);
            }
        }
    }

    bool WallAhead()
    {
        return Physics2D.Raycast(enemy.transform.position, enemy.moveDir, 1.5f, LayerMask.GetMask("Wall")).collider != null;
    }

    public override void StateFixedUpdate()
    {
        Vector2 delta = smoothVel * Time.fixedDeltaTime;
        if (enemy.Rigidbody2D != null)
            enemy.Rigidbody2D.MovePosition(enemy.Rigidbody2D.position + delta);
        else
            enemy.transform.Translate(delta, Space.World);
    }
}
