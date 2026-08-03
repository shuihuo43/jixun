using System;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class Player : MonoBehaviour
{
    #region 参数

    [Header("组件 & 资源")]
    [SerializeField] private PlayerResource playerResource;
    public WeaponResource[] WeaponResources => playerResource?.WeaponResources;
    internal int atkWeaponIdx;
    internal int atkHitCount;
    internal int atkFlipCount;
    internal float atkCooldownTimer;
    internal float weaponTurnDuration;
    internal float weaponSlotTimer; // UI fill = slotTimer / slotCd
    internal float itemTurnStart;
    internal float itemTurnDuration;
    internal int itemSlotIdx;
    [SerializeField] private Collider2D hurtCollider;
    [SerializeField] internal Transform rotateRoot;
    [SerializeField] internal Transform flipRoot;
    internal PlayerResource PlayerResource => playerResource;
    internal Collider2D HurtCollider => hurtCollider;

    [Header("移动")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float runSpeed = 16f;
    [SerializeField] private float dashSpeed = 35f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.3f;
    [SerializeField] internal AnimationCurve dashSpeedCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.3f);
    [Range(0f, 1f)] [SerializeField] private float dashEndSpeedRetention = 0.5f;
    [SerializeField] private float dashEndDecelerationTime = 0.1f;
    [SerializeField] private float accelerationTime = 0.05f;
    [SerializeField] private float decelerationTime = 0.03f;

    [Header("冲刺攻击")]
    [SerializeField] internal GameObject dashAttackEntityPrefab;
    [SerializeField] internal DamageResource dashAttackDamageResource;
    [SerializeField] internal float dashAttackSpeed = 25f;
    [SerializeField] internal float dashAttackDuration = 0.15f;
    [SerializeField] internal float dashAttackCooldown = 0.5f;
    internal float dashAttackCooldownTimer;

    [Header("攻击 & 实体")]
    [SerializeField] internal GameObject entityRoot;
    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private Transform entitySpawnPoint;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float attackScale = 1f;
    [SerializeField] private float slowMoveSpeed = 3f;
    [SerializeField] private int attackCount = 0;
    [SerializeField] private bool whenAttackMove = true;
    [SerializeField] internal bool recallBladesOnDash;
    [SerializeField] internal float recallBladeSpeed = 128f;

    [Header("奔跑相关")]
    [SerializeField] internal float runSpawnInterval = 1f;

    [Header("生命值")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    // 虚血：回血上限，受击时 = maxHealth，差值可恢复
    internal float pendingBloodMax;
    internal float pendingBloodTimer;
    private PlayerStatus playerStatus;

    #endregion

    #region 公共属性（状态机读取）

    public Vector2 MoveInput { get; internal set; }
    public Vector2 PreMovementNotZero { get; private set; } = Vector2.right;
    public float MoveSpeed => moveSpeed;
    public float RunSpeed => runSpeed;
    public float DashSpeed => dashSpeed;
    public float DashDuration => dashDuration;
    public bool IsDashing { get; set; }
    public bool IsRunning { get; set; }
    public bool IsAttacking { get; set; }
    public bool IsSlowMove { get; set; }
    public WeaponResource CurWeapon => WeaponResources is { Length: > 0 } ? WeaponResources[0] : null;
    public float SlowMoveSpeed => slowMoveSpeed;
    public bool WhenAttackMove => whenAttackMove;

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

    [Header("受击")]
    [SerializeField] private SpriteRenderer hurtSprite;
    private float hurtFadeTimer;

    internal bool canBeHurt = true;
    public int combo;
    public float comboTime { get; set; }
    public float comboMaxTime = 2f;
    public int comboStage; // 第几段连击（每 threshold 次 +1）

    public void AddCombo()
    {
        combo++;
        comboTime = comboMaxTime;
        OnComboHit?.Invoke();

        int threshold = playerResource != null ? playerResource.comboThreshold : 10;

        if (combo % threshold == 0)
        {
            comboStage++;
            if (playerResource?.comboEntities != null)
            {
                Vector2 dir = PreMovementNotZero;
                foreach (var prefab in playerResource.comboEntities)
                {
                    if (prefab == null) continue;
                    var obj = Instantiate(prefab, transform.position, Quaternion.identity);
                    var entity = obj.GetComponent<Entity>();
                    if (entity != null)
                        entity.EntityBorn(transform.position, dir, entityRoot ?? gameObject, ownerObj: gameObject);
                }
            }
        }

        if (combo % threshold == 0)
            OnComboTrigger?.Invoke();
    }

    public event Action OnComboHit;     // 每次攻击命中
    public event Action OnComboTrigger; // 每 threshold 次触发

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

        playerStatus = FindObjectOfType<PlayerStatus>();

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

        // 身体水平翻转
        if (flipRoot != null)
            UpdateFlip();

        // 3. 冲刺冷却计时
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
                canDash = true;
        }

        // 冲刺攻击冷却
        if (dashAttackCooldownTimer > 0f)
            dashAttackCooldownTimer -= Time.deltaTime;

        // 4. 冲刺（优先级最高，可打断攻击和冲刺攻击）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 攻击/冲刺攻击中按空格 → 直接冲刺（查能量，不查冷却）
            if (IsAttacking || movementSM.CurrentStateName == "DashAttack")
            {
                if (playerResource == null || ConsumeEnergy(playerResource.dashCost))
                {
                    actionSM.ChangeToState("None");
                    canDash = false;
                    dashCooldownTimer = dashCooldown;
                    movementSM.ChangeToState("Dash");
                }
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

        // 5. 冲刺中按攻击 → 打断冲刺进入攻击
        if (IsDashing && Input.GetMouseButtonDown(0))
        {
            actionSM.ChangeToState("None");
            ExecuteAttack();
            return;
        }

        // 6. 攻击
        string curMove = movementSM.CurrentStateName;
        if (!IsAttacking && curMove != "DashAttack")
        {
            // 冲刺攻击（仅奔跑中） — 暂时注释
            //if (curMove == "Run" && Input.GetMouseButtonDown(0))
            //{
            //    if (dashAttackCooldownTimer <= 0f && (playerResource == null || ConsumeEnergy(playerResource.dashCost)))
            //        movementSM.ChangeToState("DashAttack");
            //}
            if (Input.GetMouseButton(0))
            {
                ExecuteAttack();
            }
        }

        // 6. 处理预输入缓冲
        ProcessBuffer();

        // 7. 委托双状态机处理
        movementSM.StateUpdate();
        actionSM.StateUpdate();

        // 8. 连击计时
        if (combo > 0)
        {
            comboTime -= Time.deltaTime;
            if (comboTime <= 0f) { combo = 0; comboStage = 0; }
        }

        // 9. 攻击冷却计时
        if (atkCooldownTimer > 0f) atkCooldownTimer -= Time.deltaTime;

        // 10. 精力恢复
        if (playerResource != null && CurrentEnergy < playerResource.maxEnergy)
        {
            if (energyRecoveryTimer > 0f)
            {
                energyRecoveryTimer -= Time.deltaTime;
            }
            else
            {
                float prev = CurrentEnergy;
                CurrentEnergy += playerResource.maxEnergy * playerResource.recoverySpeed * Time.deltaTime;
                if (CurrentEnergy > playerResource.maxEnergy)
                    CurrentEnergy = playerResource.maxEnergy;
                if (CurrentEnergy != prev)
                    OnEnergyChanged?.Invoke();
            }
        }

        // 受击闪烁渐隐
        if (hurtFadeTimer > 0f && hurtSprite != null)
        {
            hurtFadeTimer -= Time.deltaTime;
            float t = hurtFadeTimer / 0.15f;
            var c = hurtSprite.color;
            c.a = Mathf.Lerp(0f, 0.3f, t);
            hurtSprite.color = c;
            if (hurtFadeTimer <= 0f)
                hurtSprite.enabled = false;
        }

        // 虚血超时：同步为当前血量
        if (pendingBloodTimer > 0f)
        {
            pendingBloodTimer -= Time.deltaTime;
            if (pendingBloodTimer <= 0f)
                pendingBloodMax = playerResource != null ? playerResource.currentHealth : currentHealth;
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

        // 攻击期间机动状态：不能移动则切 SlowMove
        if (!whenAttackMove)
            movementSM.ChangeToState("SlowMove");
    }

    /// <summary>当前武器槽索引和轮转进度（0~1）</summary>
    public (int slot, float fill) GetWeaponProgress()
    {
        float slotCd = weaponTurnDuration > 0 ? weaponTurnDuration : 0.2f;
        float fill = IsAttacking ? Mathf.Clamp01(weaponSlotTimer / slotCd) : 0f;
        return (atkWeaponIdx % 5, fill);
    }

    /// <summary>当前物品槽索引和轮转进度（0~1）</summary>
    public (int slot, float fill) GetItemProgress()
    {
        float elapsed = Time.time - itemTurnStart;
        float fill = itemTurnDuration > 0 ? Mathf.Clamp01(1f - elapsed / itemTurnDuration) : 0f;
        return (itemSlotIdx, fill);
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


    public void AttackLogic()
    {
        if (entityPrefab == null || entityRoot == null || entitySpawnPoint == null) return;
        if (WeaponResources.Length == 0) return;

        var weapon = WeaponResources.Length > 0 ? WeaponResources[atkWeaponIdx % WeaponResources.Length] : null;
        if (weapon == null) return;

        // 虚血恢复（需要 CanRecoverPendingBlood buff）
        float recoverable = pendingBloodMax - playerResource.currentHealth;
        bool canRecover = playerResource.HasBoolBuff(PlayerResource.BoolBuffType.CanRecoverPendingBlood);
        if (canRecover && recoverable > 0f && weapon.AttackCount > 0)
        {
            float heal = recoverable * 0.5f / weapon.AttackCount;
            playerResource.ChangeHealth(heal);
        }

        if (playerResource.healOnHit > 0 && weapon.AttackCount > 0)
            playerResource.ChangeHealth(playerResource.healOnHit / weapon.AttackCount);

        atkFlipCount++;
        bool flipY = (atkFlipCount % 2 == 1);

        GameObject entityObj = Instantiate(entityPrefab);

        var locked = FindObjectOfType<MouseCursor>()?.lockedEnemy;
        Vector2 baseDirection;
        if (locked != null)
            baseDirection = ((Vector2)locked.position - (Vector2)entitySpawnPoint.position).normalized;
        else
        {
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            baseDirection = (mouseWorldPos - entitySpawnPoint.position).normalized;
        }
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;
        float randomAngle = baseAngle + UnityEngine.Random.Range(-weapon.AttackAngleOffset, weapon.AttackAngleOffset);
        float rad = randomAngle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

        Vector2 localPos = entityRoot.transform.InverseTransformPoint(entitySpawnPoint.position);

        AttackEntity atk = entityObj.GetComponent<AttackEntity>();
        if (atk != null)
        {
            float s = playerResource != null ? playerResource.weaponScale : 0.6f;
            var scale = new Vector2(s, s);
            atk.damageResource = weapon.damageResource;
            atk.AttackBorn(weapon, localPos, dir, entityRoot, scale, flipY);
            atk.EntityBorn(localPos, dir, entityRoot, scale, flipY, null, gameObject);
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

    #region 翻转

    void UpdateFlip()
    {
        if (flipRoot == null) return;

        Vector3 scale = flipRoot.localScale;
        float playerX = transform.position.x;

        // 优先使用索敌目标，否则用鼠标
        var locked = FindObjectOfType<MouseCursor>()?.lockedEnemy;
        float targetX;
        if (locked != null)
            targetX = locked.position.x;
        else
            targetX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;

        float diff = targetX - playerX;
        if (Mathf.Abs(diff) > 5f)
            scale.x = diff > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);

        flipRoot.localScale = scale;
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

        // 虚血：先结算上一次（同步到当前血量），再存本次受击前血量
        float hpBeforeHit = playerResource != null ? playerResource.currentHealth : currentHealth;
        playerResource.ChangeHealth(-damage);
        pendingBloodMax = hpBeforeHit;
        pendingBloodTimer = 1f;

        AudioManager.Instance?.PlaySFX(hurtClip, hurtVolume);
        cameraController?.Shake(2f, 8f, 0.2f);

        if (hurtSprite != null)
        {
            hurtSprite.enabled = true;
            var c = hurtSprite.color;
            c.a = 0.3f;
            hurtSprite.color = c;
            hurtFadeTimer = 0.3f;
        }
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
