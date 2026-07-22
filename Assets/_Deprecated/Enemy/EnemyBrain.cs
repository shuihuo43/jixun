using UnityEngine;

public class EnemyBrain : MonoBehaviour
{
    [Header("移动")]
    public float moveSpeed = 3f;

    [Header("攻击")]
    public EnemyAttackEntity attack;

    [Header("索敌（IEnemyTarget 接口，由 EnemyTargeting 或其它脚本提供）")]
    [SerializeField] private MonoBehaviour targetingSource;

    private IEnemyTarget target;
    public bool CanMove { get; set; } = true;

    void Start()
    {
        target = targetingSource as IEnemyTarget;
    }

    void Update()
    {
        if (target == null || !target.IsPlayerDetected) return;
        if (attack.IsAttacking) return;
        if (!CanMove) return;

        if (target.DistanceToPlayer > attack.AttackRange)
        {
            // 追击
            Vector2 dir = target.DirectionToPlayer;
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
            transform.right = dir;
        }
        else
        {
            attack.TryAttack();
        }
    }
}
