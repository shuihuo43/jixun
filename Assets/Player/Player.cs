using UnityEngine;

public class Player : MonoBehaviour
{
    #region 参数

    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float runSpeed = 16f;
    [SerializeField] private float dashSpeed = 35f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 0.3f;

    [Header("冲刺收尾")]
    [Range(0f, 1f)]
    [SerializeField] private float dashEndSpeedRetention = 0.5f;
    [SerializeField] private float dashEndDecelerationTime = 0.1f;

    [Header("手感参数")]
    [SerializeField] private float accelerationTime = 0.05f;
    [SerializeField] private float decelerationTime = 0.03f;

    [Header("拖尾")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Transform rotateRoot;

    [Header("生命值")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("实体")]
    [SerializeField] private GameObject entityRoot;
    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private Transform entitySpawnPoint;

    [Header("攻击")]
    [SerializeField] private float attackDuration = 0.3f;
    [Range(0f, 0.5f)]
    [SerializeField] private float dashCancelRatio = 0.2f;
    [SerializeField] private float attackScale = 1f;
    [SerializeField] private float slowMoveSpeed = 3f;
    [SerializeField] private int attackCount = 0;
    [SerializeField] private bool whenAttackMove = true;
    [SerializeField] private bool whenAttackRun = true;


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
    public bool CanDashCancel { get; set; }
    public float AttackDuration => attackDuration;
    public float DashCancelRatio => dashCancelRatio;
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

    private bool canDash = true;
    private float dashCooldownTimer;

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
                if (canDash && !IsDashing && (!IsAttacking || CanDashCancel))
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
    }

    void Start()
    {
        // 机动状态
        new PlayerIdle("Idle", this, movementSM, isInit: true);
        new PlayerMove("Move", this, movementSM);
        new PlayerDash("Dash", this, movementSM);
        new PlayerRun("Run", this, movementSM);
        new PlayerSlowMove("SlowMove", this, movementSM);

        // 动作状态
        new ActionNone("None", this, actionSM, isInit: true);
        new ActionAttack("Attack", this, actionSM);
    }

    void Update()
    {
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

        // 4. 预输入：冲刺（攻击后摇最后 20% 可取消）
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool canDashNow = canDash && !IsDashing && (!IsAttacking || CanDashCancel);
            if (canDashNow)
            {
                if (IsAttacking) actionSM.ChangeToState("None"); // 打断攻击
                ExecuteDash();
            }
            else if (!canDash || IsDashing || IsAttacking)
                SetBuffer(BufferedInput.Dash);
        }

        // 5. 预输入：攻击
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsAttacking && !IsDashing)
                ExecuteAttack();
            else
                SetBuffer(BufferedInput.Attack);
        }

        // 6. 处理预输入缓冲
        ProcessBuffer();

        // 7. 委托双状态机处理
        movementSM.StateUpdate();
        actionSM.StateUpdate();
    }

    void FixedUpdate()
    {
        movementSM.StateFixedUpdate();
        actionSM.StateFixedUpdate();
        UpdateTrail();
    }

    #region 攻击方法

    /// <summary>执行攻击（调用方已通过预输入校验）</summary>
    public void ExecuteAttack()
    {
        actionSM.ChangeToState("Attack");

        // 当前是 Run → 检测是否允许攻击中奔跑，不允许则降级
        string curMove = movementSM.CurrentStateName;
        if (curMove == "Run" && !whenAttackRun)
            movementSM.ChangeToState("SlowMove");
        else if (curMove == "Move" && !whenAttackMove)
            movementSM.ChangeToState("SlowMove");
        else if (curMove != "Run" && curMove != "Move")
            movementSM.ChangeToState("SlowMove");
    }

    /// <summary>执行冲刺</summary>
    private void ExecuteDash()
    {
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

        // 获取鼠标方向向量
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseDirection = (mouseWorldPos - entitySpawnPoint.position).normalized;

        // 计算 entityRoot 下的本地坐标
        Vector2 localPos = entityRoot.transform.InverseTransformPoint(entitySpawnPoint.position);

        // 调用 EntityBorn 初始化位置和朝向
        Entity entity = entityObj.GetComponent<Entity>();
        if (entity != null)
        {
            entity.EntityBorn(localPos, mouseDirection, entityRoot, new Vector2(attackScale, attackScale), flipY);
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

        transform.Translate(currentVelocity * Time.fixedDeltaTime);
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

        transform.Translate(currentVelocity * Time.fixedDeltaTime);
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

    #endregion

    #region 拖尾

    void UpdateTrail()
    {
        if (trail == null) return;

        if (IsDashing)
            trail.colorGradient = CreateGradient(Color.red);
        else if (IsRunning)
            trail.colorGradient = CreateGradient(Color.blue);
        else
            trail.colorGradient = CreateGradient(Color.green);
    }

    Gradient CreateGradient(Color color)
    {
        return new Gradient()
        {
            colorKeys = new GradientColorKey[] {
                new GradientColorKey(color, 0f),
                new GradientColorKey(color, 1f)
            },
            alphaKeys = new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        };
    }

    #endregion
}
