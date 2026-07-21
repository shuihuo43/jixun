using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [Header("移动")]
    public float moveSpeed = 3f;
    public float acceleration = 8f;          // 加速度，越大越灵敏
    public float turnSpeed = 360f;           // 转向速度（度/秒）

    [Header("索敌走位")]
    public float detectionRange = 10f;
    public float preferredDistance = 3f;     // 偏好交战距离
    public float strafeWeight = 0.6f;        // 环绕倾向（0=直冲, 1=纯绕圈）

    [Header("攻击")]
    public EnemyAttackEntity attack;

    public Transform enemyRoot;

    private Transform player;
    private Vector2 currentVelocity;
    private float strafeSign = 1f;           // 随机左右环绕方向

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        strafeSign = Random.value > 0.5f ? 1f : -1f;
    }

    void Update()
    {
        if (player == null) return;

        Vector2 toPlayer = player.position - enemyRoot.position;
        float distance = toPlayer.magnitude;
        Vector2 dir = toPlayer / distance;

        // 攻击中不移动不转向（前摇期间站定）
        if (attack.IsAttacking) return;

        // 超出索敌范围不做任何事
        if (distance > detectionRange) return;

        // ── 平滑转向 ──
        enemyRoot.right = Vector3.RotateTowards(enemyRoot.right, dir, turnSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);

        // ── 走位策略 ──
        Vector2 moveDir;
        if (distance > preferredDistance + 0.5f)
        {
            // 太远 → 靠近
            moveDir = dir;
        }
        else if (distance < preferredDistance - 0.5f)
        {
            // 太近 → 后退
            moveDir = -dir;
        }
        else
        {
            // 合适距离 → 切向环绕
            Vector2 tangent = new Vector2(-dir.y, dir.x) * strafeSign;
            moveDir = Vector2.Lerp(dir, tangent, strafeWeight).normalized;
        }

        // ── 平滑加速移动 ──
        Vector2 targetVelocity = moveDir * moveSpeed;
        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        enemyRoot.position += (Vector3)(currentVelocity * Time.deltaTime);

        // ── 攻击 ──
        if (distance <= attack.AttackRange)
            attack.TryAttack();
    }
}
