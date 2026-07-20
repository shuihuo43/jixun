using UnityEngine;

/// <summary>
/// 攻击实体：继承 Entity，负责播放攻击动画
/// </summary>
public class AttackEntity : Entity
{
    [SerializeField] private Animator animator;

    public void AttackBorn(
        WeaponResource weaponResource,
        Vector2 position,
        Vector2 direction,
        GameObject bornRoot,
        Vector2? scale = null,
        bool flipY = false)
    {
        // 基类处理位置/朝向/缩放
        EntityBorn(position, direction, bornRoot, scale, flipY);

        // 子类：设置攻击动画
        if (animator != null && weaponResource != null)
        {
            animator.SetInteger("AttackIndex", (int)weaponResource.AttackType);
            animator.SetTrigger("Attack");
        }
    }
}
