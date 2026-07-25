using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Player : MonoBehaviour
{
    #region 参数

    [Header("组件 & 资源")]
    [SerializeField] private PlayerResource playerResource;
    [SerializeField] private WeaponResource curWeapon;
    [SerializeField] private Collider2D hurtCollider;
    [SerializeField] internal Transform rotateRoot;
    internal PlayerResource PlayerResource => playerResource;
    internal Collider2D HurtCollider => hurtCollider;

    [Header("移动")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float runSpeed = 16f;
    [SerializeField] private float dashSpeed = 35f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.3f;
    [Range(0f, 1f)] [SerializeField] private float dashEndSpeedRetention = 0.5f;
    [SerializeField] private float dashEndDecelerationTime = 0.1f;
    [SerializeField] private float accelerationTime = 0.05f;
    [SerializeField] private float decelerationTime = 0.03f;

    [Header("冲刺攻击")]
    [SerializeField] internal GameObject dashAttackEntityPrefab;
    [SerializeField] internal float dashAttackSpeed = 25f;
    [SerializeField] internal float dashAttackDuration = 0.15f;

    [Header("攻击 & 实体")]
    [SerializeField] internal GameObject entityRoot;
    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private Transform entitySpawnPoint;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float attackScale = 1f;
    [SerializeField] private float slowMoveSpeed = 3f;
    [SerializeField] private int attackCount = 0;
    [SerializeField] private bool whenAttackMove = true;
    [SerializeField] private bool whenAttackRun = true;

    [Header("奔跑相关")]
    [SerializeField] internal GameObject[] runSpawnPrefabs;
    [SerializeField] internal float runSpawnInterval = 1f;

    [Header("生命值")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    internal float runSpawnTimer;
    internal int runSpawnIndex;

    #endregion

    #region 公共属性（状态机读取）

    public Vector2 MoveInput { get; private set; }
    public Vector2 PreMovementNotZero { get; private set; } = Vector2.right;
    public float MoveSpeed => moveSpeed;
    public float RunSpeed => runSpeed;
    public float DashSpeed => dashSpeed;
    public float DashDuration => dashDuration;
    public bool IsDashing { get; set; }
    public bool IsRunning { get; set; }
    public bool IsAttacking { get; set; }
    public bool IsSlowMove { get; set; }
    public WeaponResource CurWeapon => curWeapon;
    public float AttackDuration => curWeapon != null ? curWeapon.TotalDuration : attackDuration;
    public float AttackBaseDuration => curWeapon != null ? curWeapon.AttackDuration : attackDuration;
    public float SlowMoveSpeed => slowMoveSpeed;
    public bool WhenAttackMove => whenAttackMove;
    public bool WhenAttackRun => whenAttackRun;

    // 生命值属性（外部UI/Enemy使用）
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    #endregion

    #region 内部状态（状态机共享）

    internal Vector2 currentVelocity;
    internal Vector2 velocityRef;

    private Rigidbody2D rb;
    internal Rigidbody2D Rigidbody2D => rb;
    [Header("音效")]
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private float hurtVolume = 0.8f;
    [SerializeField] internal AudioClip dashClip;
    [SerializeField] internal float dashVolume = 0.8f;

    internal bool canBeHurt = true;
    internal float dashAttackBuffer;
    public int combo;
    public float comboTime { get; set; }
    public float comboMaxTime = 2f;

    public CameraController cameraController;

    internal bool canDash = true;
    private float dashCooldownTimer;

    // 精力（运行时状态，配置在 PlayerResource ScriptableObject 中）
    public event Action OnHealthChanged;
    public event Action OnEnergyChanged;
    public float CurrentEnergy { get; private set; }
    public float MaxEnergy => playerResource != null ? playerResource.maxEnergy : 0f;
    private float energyRecoveryTimer;

    /// <summary>消耗精力，不足返回 false</summary>
    public bool ConsumeEnergy(float amount)
    {
        if (playerResource == null || CurrentEnergy < amount) return false;

        CurrentEnergy -= amount;
        energyRecoveryTimer = playerResource.recoveryInterval;
        OnEnergyChanged?.Invoke();
        return true;
    }

    #endregion

    #region 预输入

    private enum BufferedInput { None, Attack, Dash }

    [Header("预输入")]
    [SerializeField] private float bufferWindow = 0.25f;

    private BufferedInput bufferedInput = BufferedInput.None;
    private float bufferTimer;

    /// <summary>尝试存入预输入，已有则忽略</summary>
    private void SetBuffer(BufferedInput input)
    {
        if (bufferedInput == BufferedInput.None)
        {
            bufferedInput = input;
            bufferTimer = bufferWindow;
        }
    }

    /// <summary>每帧尝试消费预输入</summary>
    private void ProcessBuffer()
    {
        if (bufferedInput == BufferedInput.None) return;

        bufferTimer -= Time.deltaTime;
        if (bufferTimer <= 0f)
        {
            bufferedInput = BufferedInput.None;
            return;
        }

        switch (bufferedInput)
        {
            case BufferedInput.Attack:
                if (!IsAttacking && !IsDashing)
                {
                    bufferedInput = BufferedInput.None;
                    ExecuteAttack();
                }
                break;
            case BufferedInput.Dash:
                if (canDash && !IsDashing)
                {
                    bufferedInput = BufferedInput.None;
                    if (IsAttacking) actionSM.ChangeToState("None");
                    ExecuteDash();
                }
                break;
        }
    }

    #endregion

    #region 状态机

    private PlayerStateMachine movementSM;
    private ActionStateMachine actionSM;

    #endregion

    void Awake()
    {
        movementSM = GetComponent<PlayerStateMachine>();
        if (movementSM == null)
            movementSM = gameObject.AddComponent<PlayerStateMachine>();

        actionSM = new ActionStateMachine();

        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        // 机动状态
        new PlayerIdle("Idle", this, movementSM, isInit: true);
        new PlayerMove("Move", this, movementSM);
        new PlayerDash("Dash", this, movementSM);
        new PlayerDashAttack("DashAttack", this, movementSM);
        new PlayerRun("Run", this, movementSM);
        new PlayerSlowMove("SlowMove", this, movementSM);

        // 动作状态
        new ActionNone("None", this, actionSM, isInit: true);
        new ActionAttack("Attack", this, actionSM);

        // 相机引用
        var camObj = GameObject.FindGameObjectWithTag("Camera");
        if (camObj != null)
            cameraController = camObj.GetComponent<CameraController>();

        // 运行时拷贝资源，不污染原 ScriptableObject
        if (playerResource != null)
        {
            playerResource = Instantiate(playerResource);
            playerResource.OnHealthChanged += () => OnHealthChanged?.Invoke();
            CurrentEnergy = playerResource.maxEnergy;
            playerResource.currentHealth = playerResource.maxHealth;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing) return;

        // 1. 鼠标跟随
        Utilties.FollowMouse(rotateRoot, 90, 0);

        // 2. 读取输入
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        MoveInput = new Vector2(h, v).normalized;

        if (MoveInput != Vector2.zero)
            PreMovementNotZero = MoveInput;

        // 3. 冲刺冷却计时
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
                canDash = true;
        }

        // 4. 冲刺（优先级最高，可打断攻击和冲刺攻击）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 冲刺攻击中按空格 → 切回冲刺（查能量，不查冷却）
            if (movementSM.CurrentStateName == "DashAttack")
            {
                if (playerResource == null || ConsumeEnergy(playerResource.dashCost))
                    movementSM.ChangeToState("Dash");
                return;
            }

            if (canDash && !IsDashing)
            {
                if (IsAttacking) actionSM.ChangeToState("None");
                ExecuteDash();
            }
            else if (!canDash || IsDashing)
                SetBuffer(BufferedInput.Dash);
        }

        // 5. 冲刺攻击（冲刺中 或 缓冲期内）
        dashAttackBuffer -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && (IsDashing || dashAttackBuffer > 0f))
        {
            movementSM.ChangeToState("DashAttack");
            return;
        }

        // 6. 预输入：攻击
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsAttacking && !IsDashing)
                ExecuteAttack();
            else
                SetBuffer(BufferedInput.Attack);
        }

        // 7. 处理预输入缓冲
        ProcessBuffer();

        // 8. 委托双状态机处理
        movementSM.StateUpdate();
        actionSM.StateUpdate();

        // 8. 连击计时
        if (combo > 0)
        {
            comboTime -= Time.deltaTime;
            if (comboTime <= 0f) combo = 0;
        }

        // 9. 精力恢复
        if (playerResource != null && CurrentEnergy < playerResource.maxEnergy)
        {
            if (energyRecoveryTimer > 0f)
            {
                energyRecoveryTimer -= Time.deltaTime;
            }
            else
            {
                float prev = CurrentEnergy;
                CurrentEnergy += playerResource.recoverySpeed * Time.deltaTime;
                if (CurrentEnergy > playerResource.maxEnergy)
                    CurrentEnergy = playerResource.maxEnergy;
                if (CurrentEnergy != prev)
                    OnEnergyChanged?.Invoke();
            }
        }
    }

    void FixedUpdate()
    {
        movementSM.StateFixedUpdate();
        actionSM.StateFixedUpdate();
    }

    #region 攻击方法

    /// <summary>执行攻击（调用方已通过预输入校验）</summary>
    public void ExecuteAttack()
    {
        actionSM.ChangeToState("Attack");

        // 攻击期间机动状态降级：Run→Move→SlowMove
        string curMove = movementSM.CurrentStateName;
        if (curMove == "Run")
        {
            if (!whenAttackRun)
                movementSM.ChangeToState(whenAttackMove ? "Move" : "SlowMove");
        }
        else if (curMove == "Move")
        {
            if (!whenAttackMove)
                movementSM.ChangeToState("SlowMove");
        }
        else
        {
            movementSM.ChangeToState("SlowMove");
        }
    }

    /// <summary>执行冲刺</summary>
    private void ExecuteDash()
    {
        // 精力不足无法冲刺
        if (playerResource != null && !ConsumeEnergy(playerResource.dashCost))
            return;

        canDash = false;
        dashCooldownTimer = dashCooldown;
        movementSM.ChangeToState("Dash");
    }


    // 攻击逻辑
    public void AttackLogic()
    {
        if (entityPrefab == null || entityRoot == null || entitySpawnPoint == null)
            return;

        // 交替 flipY
        attackCount++;
        bool flipY = (attackCount % 2 == 1);

        // 实例化实体
        GameObject entityObj = Instantiate(entityPrefab);

        // 获取鼠标方向向量，加上武器随机偏移角度
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 baseDirection = (mouseWorldPos - entitySpawnPoint.position).normalized;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        float offsetRange = curWeapon != null ? curWeapon.AttackAngleOffset : 0f;
        float randomAngle = baseAngle + UnityEngine.Random.Range(-offsetRange, offsetRange);
        float rad = randomAngle * Mathf.Deg2Rad;
        Vector2 mouseDirection = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        // 计算 entityRoot 下的本地坐标
        Vector2 localPos = entityRoot.transform.InverseTransformPoint(entitySpawnPoint.position);

        // 调用 AttackBorn 初始化
        AttackEntity attackEntity = entityObj.GetComponent<AttackEntity>();
        if (attackEntity != null)
        {
            attackEntity.AttackBorn(curWeapon, localPos, mouseDirection, entityRoot, new Vector2(attackScale, attackScale), flipY);
            attackEntity.EntityBorn(localPos, mouseDirection, entityRoot, ownerObj: gameObject);
            if (curWeapon != null)
                attackEntity.damageResource = curWeapon.damageResource;
        }
    }

        #endregion

    #region 移动方法（状态机调用）

        /// <summary>
        /// 带加速度的平滑移动，Move/Run 状态在 FixedUpdate 调用
        /// </summary>
    public void ApplyMovement(float targetSpeed)
    {
        Vector2 targetVelocity = MoveInput * targetSpeed;

        currentVelocity = Vector2.SmoothDamp(
            currentVelocity,
            targetVelocity,
            ref velocityRef,
            accelerationTime
        );

        // 超速钳制
        if (currentVelocity.magnitude > targetSpeed)
            currentVelocity = currentVelocity.normalized * targetSpeed;

        Vector2 delta = currentVelocity * Time.fixedDeltaTime;
        if (rb != null)
            rb.MovePosition(rb.position + delta);
        else
            transform.Translate(delta);
    }

    /// <summary>
    /// 速度衰减至0，Idle 状态在 FixedUpdate 调用
    /// </summary>
    public void ApplyDeceleration()
    {
        currentVelocity = Vector2.SmoothDamp(
            currentVelocity,
            Vector2.zero,
            ref velocityRef,
            decelerationTime
        );

        if (currentVelocity.magnitude < 0.1f)
            currentVelocity = Vector2.zero;

        Vector2 delta = currentVelocity * Time.fixedDeltaTime;
        if (rb != null)
            rb.MovePosition(rb.position + delta);
        else
            transform.Translate(delta);
    }

    /// <summary>
    /// 冲刺结束时设定保留速度，Dash 状态退出前调用
    /// </summary>
    public void SetDashEndVelocity(Vector2 dashDir)
    {
        float retainedSpeed = Mathf.Lerp(moveSpeed, dashSpeed, dashEndSpeedRetention);
        currentVelocity = dashDir * retainedSpeed;
    }

    #endregion

    #region 生命值

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        if (currentHealth <= 0f)
        {
            Debug.Log("玩家死亡");
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("EnemyAttack")) return;
        Entity src = other.GetComponent<Entity>();
        Hurt(src);
    }

    public void Hurt(Entity source)
    {
        if (!canBeHurt) return;

        float damage = source?.damageResource != null ? source.damageResource.baseDamageValue : 1f;
        playerResource.ChangeHealth(-damage);
        AudioManager.Instance?.PlaySFX(hurtClip, hurtVolume);
        cameraController?.Shake(12f, 8f, 0.2f);
    }

    #endregion

    #region 拖尾（调试 / 已注释）

    // [Header("拖尾")]
    // [SerializeField] private TrailRenderer trail;

    // FixedUpdate 中调用:
    // UpdateTrail();

    // void UpdateTrail()
    // {
    //     if (trail == null) return;
    //
    //     if (IsDashing)
    //         trail.colorGradient = CreateGradient(Color.red);
    //     else if (IsRunning)
    //         trail.colorGradient = CreateGradient(Color.blue);
    //     else
    //         trail.colorGradient = CreateGradient(Color.green);
    // }
    //
    // Gradient CreateGradient(Color color)
    // {
    //     return new Gradient()
    //     {
    //         colorKeys = new GradientColorKey[] {
    //             new GradientColorKey(color, 0f),
    //             new GradientColorKey(color, 1f)
    //         },
    //         alphaKeys = new GradientAlphaKey[] {
    //             new GradientAlphaKey(1f, 0f),
    //             new GradientAlphaKey(1f, 1f)
    //         }
    //     };
    // }

    #endregion
}
