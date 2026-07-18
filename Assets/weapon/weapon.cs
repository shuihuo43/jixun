using UnityEngine;

public class WeaponFollowMouse2D : MonoBehaviour
{
    [Header("环绕设置")]
    public Transform player;
    public float radius = 1.5f;

    [Header("攻击属性")]
    public int damage = 10;
    public float attackRange = 1f;          // 攻击范围半径
    public LayerMask enemyLayer;            // 敌人所在层

    [Header("动画")]
    public Animator animator;               // 武器的 Animator（如果有）
    // 如果没有动画器，可以注释掉相关代码，改用等待时间

    private bool isAttacking = false;

    void Update()
    {
        if (player == null) return;

        // === 1. 环绕逻辑（保持原有） ===
        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = -Camera.main.transform.position.z;
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

        Vector2 direction = (mouseWorldPos - player.position).normalized;
        transform.position = player.position + (Vector3)(direction * radius);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle - 45); // 保留你之前的偏移

        // === 2. 攻击输入 ===
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            Attack();
        }
    }

    void Attack()
    {
        isAttacking = true;

        // 触发攻击动画
        if (animator != null)
            animator.SetTrigger("Attack");

        // 造成伤害：检测范围内的所有敌人
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            transform.position,
            attackRange,
            enemyLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            // 假设敌人脚本有 TakeDamage(int) 方法，可根据你的项目修改
            enemy.GetComponent<Enemy>()?.TakeDamage(damage);
            // 如果想用接口： enemy.GetComponent<IDamageable>()?.TakeDamage(damage);
        }
    }

    // 由动画事件调用，用于结束攻击状态
    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    // 可视化攻击范围（仅 Scene 视图可见）
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}