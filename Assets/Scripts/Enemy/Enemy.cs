using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("组件")]
    public EnemyStateMachine stateMachine;
    public EnemyPathfinding pathfinding;
    [SerializeField] private Collider2D hitCollider;

    [Header("资源")]
    public EnemyResource resource;

    [Header("朝向")]
    public Transform rotateRoot;

    [Header("属性")]
    public float moveSpeed = 3f;
    public float turnSpeed = 360f;

    [Header("音效")]
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private float hurtVolume = 0.8f;
    public float knockbackForce = 5f;
    public float knockbackAngleOffset = 15f;

    /// <summary>墙壁检测距离</summary>
    [SerializeField] private float wallDetectRange = 1.5f;
    /// <summary>墙壁避让力度</summary>
    [SerializeField] private float wallAvoidWeight = 3f;

    [Header("索敌")]
    public float detectionRadius = 8f;


    [Header("攻击")]
    public ShapeArea attackShape;
    public float attackCooldown = 1f;
    public GameObject attackAlertPrefab;
    public Transform attackAlertSpawnPoint;


    public bool IsAttackColdDown { get; private set; }
    public bool IsPlayerInAttackRange { get; private set; }


    private float attackCooldownTimer;


    public bool IsPlayerDetected { get; private set; }

    public Transform Player { get; private set; }

    /// <summary>交战状态名（子类可重写，如 Boss 用 Follow）</summary>
    public virtual string EngageStateName => "Engage";
    /// <summary>是否进入受击状态（Boss 重写为 false）</summary>
    public virtual bool UseHurtState => true;
    /// <summary>最后一次受击是否为玩家攻击</summary>
    public bool LastHitFromPlayer { get; private set; }


    [HideInInspector]
    public Vector2 moveDir;

    [HideInInspector]
    public Vector2 faceDir;


    private Rigidbody2D rb;

    internal Rigidbody2D Rigidbody2D => rb;


    private readonly Dictionary<string, float> hitRecords = new();

    private float bleedTickTimer;
    private const float bleedTickInterval = 0.2f;

    void Awake()
    {
        if (stateMachine == null)
            stateMachine = GetComponent<EnemyStateMachine>();

        rb = GetComponent<Rigidbody2D>();

        if (resource != null)
        {
            resource = Instantiate(resource);
            resource.currentHealth = resource.maxHealth;
            resource.OnReap += () =>
            {
                ReleaseAllWraithMarks();
            };

            resource.OnDeath += () =>
            {
                // 击杀回血
                var pr = Player?.GetComponent<Player>()?.PlayerResource;
                if (pr != null && pr.healOnKill > 0)
                    pr.ChangeHealth(pr.healOnKill);

                ReleaseAllWraithMarks();
                resource.statusDict[DebuffType.WraithMark] = 0;
                stateMachine.ChangeToState("Death");
            };
        }

        GameObject obj =
            GameObject.FindGameObjectWithTag("Player");


        if (obj != null)
            Player = obj.transform;

    }



    void Update()
    {
        if (stateMachine.CurrentStateName == "Death") return;

        // 动态同步 PlayerResource 上限（支持词条变更）
        var pr = Player?.GetComponent<Player>()?.PlayerResource;
        if (resource != null && pr != null)
        {
            resource.bleedMaxStacks = pr.bleedMaxStacks;
            resource.wraithMaxStacks = pr.wraithMaxStacks;
            resource.bleedDuration = pr.bleedDuration;
        }

        UpdateBleedTick();
        resource?.UpdateBleedExpiry(Time.deltaTime);
        UpdateAttackCooldown();

        UpdateRotation();


        if (Player != null)
        {
            float distance =
                Vector2.Distance(
                    transform.position,
                    Player.position);


            IsPlayerDetected =
                distance <= detectionRadius &&
                HasLineOfSight();


            IsPlayerInAttackRange =
                CheckAttackRange();


            if (IsPlayerDetected)
                OnPlayerDetected();
        }

        ApplyWallAvoidance();
    }

    void ApplyWallAvoidance()
    {
        if (moveDir.sqrMagnitude < 0.01f) return;

        var hit = Physics2D.Raycast(transform.position, moveDir, wallDetectRange, LayerMask.GetMask("Wall"));
        if (hit.collider == null) return;

        float t = 1f - (hit.distance / wallDetectRange); // 越近权重越高
        Vector2 away = hit.normal * (t * wallAvoidWeight);
        moveDir = (moveDir + away).normalized;
    }



    void UpdateAttackCooldown()
    {
        if (!IsAttackColdDown)
            return;


        attackCooldownTimer -= Time.deltaTime;


        if (attackCooldownTimer <= 0)
        {
            attackCooldownTimer = 0;
            IsAttackColdDown = false;
        }
    }



    public void StartAttackCooldown()
    {
        IsAttackColdDown = true;
        attackCooldownTimer = attackCooldown;
    }



    bool CheckAttackRange()
    {
        if (attackShape == null || Player == null)
            return false;


        Vector2 dir =
            Player.position -
            transform.position;


        float distance =
            dir.magnitude;



        if (attackShape.Shape ==
           ShapeArea.ShapeType.Sector)
        {
            if (distance > attackShape.Radius)
                return false;


            float targetAngle =
                Mathf.Atan2(
                    dir.y,
                    dir.x)
                * Mathf.Rad2Deg;


            float currentAngle =
                rotateRoot.eulerAngles.z;


            return Mathf.Abs(
                Mathf.DeltaAngle(
                    currentAngle,
                    targetAngle))
                <= attackShape.Angle / 2f;
        }



        if (attackShape.Shape ==
           ShapeArea.ShapeType.Box)
        {
            return
                Mathf.Abs(dir.x)
                <= attackShape.BoxWidth / 2f
                &&
                Mathf.Abs(dir.y)
                <= attackShape.BoxHeight / 2f;
        }


        return false;
    }




    bool HasLineOfSight()
    {
        Vector2 dir =
            Player.position -
            transform.position;


        RaycastHit2D hit =
            Physics2D.Raycast(
                transform.position,
                dir.normalized,
                dir.magnitude,
                LayerMask.GetMask("Wall"));


        return hit.collider == null;
    }




    void UpdateRotation()
    {
        if (rotateRoot == null)
            return;


        if (faceDir.sqrMagnitude < 0.001f)
            return;



        float target =
            Mathf.Atan2(
                faceDir.y,
                faceDir.x)
            * Mathf.Rad2Deg;


        float current =
            rotateRoot.eulerAngles.z;



        rotateRoot.rotation =
            Quaternion.Euler(
                0,
                0,
                Mathf.MoveTowardsAngle(
                    current,
                    target,
                    turnSpeed *
                    Time.deltaTime));
    }




    protected virtual void OnPlayerDetected()
    {

    }





    /// <summary>统一幽灵生成入口，数量受 PlayerResource.GhostSpawnCount 影响</summary>
    void SpawnGhosts(int count)
    {
        var ghostPrefab = GameManager.Instance?.GetEntity("幽灵");
        if (ghostPrefab == null) return;

        var pr = Player?.GetComponent<Player>()?.PlayerResource;
        int multiplier = pr != null ? pr.ghostSpawnCount : 1;
        int total = count * multiplier;

        var root = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
        if (root == null) root = gameObject;

        for (int i = 0; i < total; i++)
        {
            var obj = Instantiate(ghostPrefab, transform.position, Quaternion.identity);
            var entity = obj.GetComponent<Entity>();
            if (entity != null)
                entity.EntityBorn(root.transform.InverseTransformPoint(transform.position), Random.insideUnitCircle.normalized, root);
        }
    }

    void ReleaseAllWraithMarks()
    {
        if (resource == null) return;
        if (!resource.statusDict.TryGetValue(DebuffType.WraithMark, out int mark) || mark <= 0) return;
        SpawnGhosts(mark);
    }

    public void TakeDamage(float damage, DamageType damageType)
    {
        // 增伤计算
        var pr = Player?.GetComponent<Player>()?.PlayerResource;
        float bonus = pr?.damageBonuses?[damageType] ?? 1f;
        float finalDmg = damage * bonus;

        resource?.ChangeHealth(-finalDmg);

        // 跳字位置：从敌人向远离玩家方向偏移
        Vector2 spawnPos = transform.position;
        var dnc = DamageNumberCanvas.Instance;
        if (Player != null && dnc != null)
        {
            Vector2 awayDir = ((Vector2)transform.position - (Vector2)Player.position).normalized;
            spawnPos += awayDir * dnc.OffsetDistance;
        }
        dnc?.Spawn(spawnPos, Mathf.RoundToInt(finalDmg), damageType);
    }

    void UpdateBleedTick()
    {
        if (resource == null) return;
        if (!resource.statusDict.TryGetValue(DebuffType.Blood, out int stacks) || stacks <= 0) return;

        bleedTickTimer += Time.deltaTime;
        if (bleedTickTimer < bleedTickInterval) return;
        bleedTickTimer -= bleedTickInterval;

        var pr = Player?.GetComponent<Player>()?.PlayerResource;
        float bleedDmg = pr != null ? pr.bleedDamage : 5f;
        TakeDamage(stacks * bleedDmg, DamageType.Bleed);
    }




    void OnTriggerEnter2D(Collider2D other)
    {
        if (stateMachine.CurrentStateName == "Death") return;

        int layer = other.gameObject.layer;
        bool isPlayerAtk = layer == LayerMask.NameToLayer("PlayerAttack");
        bool isElementAtk = layer == LayerMask.NameToLayer("ElementAttack");
        if (!isPlayerAtk && !isElementAtk) return;

        string key = other.tag;
        if (hitRecords.TryGetValue(key, out float t) && Time.time < t) return;
        hitRecords[key] = Time.time + 0.05f;

        Entity source = other.GetComponent<Entity>();
        LastHitFromPlayer = isPlayerAtk;

        // 先结算收割/异常状态，再结算伤害，确保 Reap 在死亡前处理
        var debuffs = source?.damageResource?.GetDebuffDict();
        Debug.Log($"[Debuff] source={source}, dmgRes={source?.damageResource}, debuffs={debuffs?.Count ?? 0}");
        resource?.ApplyDebuffs(debuffs);

        float dmg = source?.damageResource != null ? source.damageResource.baseDamageValue : 1f;
        DamageType dtype = source?.damageResource != null ? source.damageResource.damageType : DamageType.Physics;
        TakeDamage(dmg, dtype);

        // 玩家攻击 + 有冤魂标记 → 生成幽灵并消耗一层
        if (isPlayerAtk && resource != null && resource.statusDict.TryGetValue(DebuffType.WraithMark, out int mark) && mark > 0)
        {
            SpawnGhosts(1);
            resource.statusDict[DebuffType.WraithMark] = mark - 1;
        }

        if (source?.owner != null)
        {
            var p = source.owner.GetComponent<Player>();
            if (p != null) { p.AddCombo(); }
        }

        AudioManager.Instance?.PlaySFX(hurtClip, hurtVolume);

        if (!isPlayerAtk) return;

        if (rb != null)
        {
            Vector2 away = ((Vector2)transform.position - (Vector2)Player.position).normalized;
            float angle = Mathf.Atan2(away.y, away.x) * Mathf.Rad2Deg;
            angle += Random.Range(-knockbackAngleOffset, knockbackAngleOffset);
            float rad = angle * Mathf.Deg2Rad;
            away = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            rb.velocity = away * knockbackForce;
        }

        if (UseHurtState) stateMachine.ChangeToState("Hurt");
    }
}