using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("移动")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("冲刺")]
    [SerializeField] private float dashSpeed = 15f;         // 冲刺速度
    [SerializeField] private float dashDuration = 0.2f;     // 冲刺持续时间
    [SerializeField] private float dashCooldown = 0.8f;     // 冲刺冷却时间
    [SerializeField] private float dashCost = 20f;          // 冲刺消耗精力
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float cooldownTimer = 0f;
    private Vector2 dashDirection;
    private Vector2 lastMoveDirection = Vector2.down;       // 默认朝下

    [Header("加速（长按空格）")]
    [SerializeField] private float accelerateChargeTime = 0.3f;      // 长按蓄力时间
    [SerializeField] private float accelerateSpeedMultiplier = 1.5f; // 加速移速倍数
    [SerializeField] private float accelerateStaminaCostPerSec = 30f;// 加速每秒精力消耗
    private bool isAccelerating = false;
    private float spaceHoldTime = 0f;        // 空格已按下时间
    private bool spacePressed = false;       // 空格当前是否按下
    private bool accelerateTriggered = false;// 本次按下是否已触发加速（防止精力耗尽后反复触发）

    [Header("生命值")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth = 100f;

    [Header("精力值")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float staminaRegenPerSecond = 25f;  // 精力自动回复速度
    [SerializeField] private float staminaRegenDelay = 0.5f;     // 消耗后延迟回复时间
    private float staminaRegenTimer = 0f;

    // 公共属性（外部UI等使用）
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float MaxStamina => maxStamina;
    public float CurrentStamina => currentStamina;
    public bool IsDashing => isDashing;
    public bool IsAccelerating => isAccelerating;   // 新增：暴露加速状态
    public Vector2 LastMoveDirection => lastMoveDirection;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        // 读取移动输入
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(horizontal, vertical).normalized;

        if (moveInput != Vector2.zero)
            lastMoveDirection = moveInput;

        // ---------- 空格输入处理（区分短按冲刺与长按加速）----------
        // 按下空格
        if (Input.GetKeyDown(KeyCode.Space))
        {
            spacePressed = true;
            spaceHoldTime = 0f;
            accelerateTriggered = false;
        }

        // 按住空格
        if (Input.GetKey(KeyCode.Space) && spacePressed)
        {
            spaceHoldTime += Time.deltaTime;

            // 满足加速条件：不在冲刺、不在加速、长按时间达标、本次未触发过加速、精力>0
            if (!isDashing && !isAccelerating &&
                spaceHoldTime >= accelerateChargeTime &&
                !accelerateTriggered &&
                currentStamina > 0f)
            {
                isAccelerating = true;
                accelerateTriggered = true;
            }
        }

        // 松开空格
        if (Input.GetKeyUp(KeyCode.Space))
        {
            // 松键时若未触发加速且短按时间满足，执行原有的冲刺
            if (!isDashing && !isAccelerating &&
                spaceHoldTime < accelerateChargeTime &&
                cooldownTimer <= 0f &&
                currentStamina >= dashCost)
            {
                StartDash();
            }

            // 无论是否触发加速，松开空格都结束加速状态
            if (isAccelerating)
            {
                isAccelerating = false;
            }

            spacePressed = false;
            spaceHoldTime = 0f;
        }

        // ---------- 加速持续消耗精力 ----------
        if (isAccelerating)
        {
            currentStamina -= accelerateStaminaCostPerSec * Time.deltaTime;
            // 加速期间持续重置回复延迟
            staminaRegenTimer = staminaRegenDelay;

            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isAccelerating = false; // 精力耗尽强制结束加速
            }
        }

        // ---------- 精力自动回复 ----------
        if (currentStamina < maxStamina && !isAccelerating) // 加速时不自动回复（已持续消耗）
        {
            if (staminaRegenTimer <= 0f)
                currentStamina += staminaRegenPerSecond * Time.deltaTime;
            else
                staminaRegenTimer -= Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }

        // ---------- 冲刺计时器 ----------
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                isDashing = false;
        }

        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            // 冲刺：固定方向高速移动
            rb.velocity = dashDirection * dashSpeed;
        }
        else if (isAccelerating)
        {
            // 加速状态：移动速度乘以倍数
            rb.velocity = moveInput * moveSpeed * accelerateSpeedMultiplier;
        }
        else
        {
            // 普通移动
            rb.velocity = moveInput * moveSpeed;
        }
    }

    // 原有的冲刺启动方法（内部调用）
    private void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        cooldownTimer = dashCooldown;

        currentStamina -= dashCost;
        staminaRegenTimer = staminaRegenDelay;

        dashDirection = moveInput != Vector2.zero ? moveInput : lastMoveDirection;
    }

    // ---------- 公共方法（供其他系统调用）----------

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        staminaRegenTimer = staminaRegenDelay;
    }

    public void ModifyDashCost(float newCost)
    {
        dashCost = Mathf.Max(0f, newCost);
    }

    public void AddTemporaryDashCostModifier(float modifier)
    {
        dashCost += modifier;
        dashCost = Mathf.Max(0f, dashCost);
    }
}