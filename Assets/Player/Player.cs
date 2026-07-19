using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 12f;
    [SerializeField] private float runSpeed = 18f;
    [SerializeField] private float dashSpeed = 45f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.3f;

    [Header("冲刺手感")]
    [Range(0f, 1f)]
    [SerializeField] private float dashEndSpeedRetention = 0.6f;   // 冲刺结束保留多少速度
    [SerializeField] private float dashEndDecelerationTime = 0.1f; // 冲刺结束减速过渡时间

    [Header("手感参数")]
    [SerializeField] private float accelerationTime = 0.05f;
    [SerializeField] private float decelerationTime = 0.03f;

    [Header("组件")]
    [SerializeField] private TrailRenderer trail;
    [SerializeField] private Transform rotateRoot;

    // 私有变量
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private Vector2 dashDirection;
    private Vector2 preMovementNotZero = Vector2.right; // 记录上次移动方向

    private bool isDash;
    private bool isRun;
    private bool canDash = true;

    private float dashTimer;
    private float dashCooldownTimer;

    private Vector2 velocityRef;

    // Trail颜色
    private Color dashDebugColor = Color.red;
    private Color normalDebugColor = Color.green;
    private Color runDebugColor = Color.blue;

    void Start()
    {
        dashTimer = -1f;
    }

    void Update()
    {
        Utilties.FollowMouse(rotateRoot, 90, 0);

        // 获取输入
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput = moveInput.normalized;

        // 记录移动方向
        if (moveInput != Vector2.zero)
            preMovementNotZero = moveInput;

        // 冲刺冷却计时
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
            {
                canDash = true;
            }
        }

        // 冲刺输入
        if (Input.GetKeyDown(KeyCode.Space) && canDash && !isDash)
        {
            StartDash();
        }

        // 跑步检测
        if (isRun && !Input.GetKey(KeyCode.Space))
        {
            isRun = false;
        }
    }

    void FixedUpdate()
    {
        // 冲刺计时
        if (isDash)
        {
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }

        // 应用移动
        if (isDash)
        {
            // 冲刺：固定方向、固定速度
            transform.Translate(dashDirection * dashSpeed * Time.fixedDeltaTime);
        }
        else
        {
            ApplyMovement();
        }

        UpdateTrail();
    }

    void StartDash()
    {
        isDash = true;
        canDash = false;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        // 方向逻辑：有输入用输入，无输入用上次移动方向
        if (moveInput != Vector2.zero)
            dashDirection = moveInput;
        else
            dashDirection = preMovementNotZero;
    }

    void EndDash()
    {
        isDash = false;
        dashTimer = -1f;

        // 冲刺结束，按住空格进入奔跑
        if (Input.GetKey(KeyCode.Space))
        {
            isRun = true;
        }

        // 冲刺结束保留速度
        float retainedSpeed = Mathf.Lerp(moveSpeed, dashSpeed, dashEndSpeedRetention);
        currentVelocity = dashDirection * retainedSpeed;
    }

    void ApplyMovement()
    {
        float targetSpeed = isRun ? runSpeed : moveSpeed;

        if (moveInput.magnitude > 0.1f)
        {
            Vector2 targetVelocity = moveInput * targetSpeed;
            currentVelocity = Vector2.SmoothDamp(
                currentVelocity,
                targetVelocity,
                ref velocityRef,
                accelerationTime
            );
        }
        else
        {
            currentVelocity = Vector2.SmoothDamp(
                currentVelocity,
                Vector2.zero,
                ref velocityRef,
                decelerationTime
            );

            if (currentVelocity.magnitude < 0.1f)
                currentVelocity = Vector2.zero;
        }

        if (currentVelocity.magnitude > targetSpeed)
        {
            currentVelocity = currentVelocity.normalized * targetSpeed;
        }

        transform.Translate(currentVelocity * Time.fixedDeltaTime);
    }

    void UpdateTrail()
    {
        if (isDash)
            trail.colorGradient = CreateGradient(dashDebugColor);
        else if (isRun)
            trail.colorGradient = CreateGradient(runDebugColor);
        else
            trail.colorGradient = CreateGradient(normalDebugColor);
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
}