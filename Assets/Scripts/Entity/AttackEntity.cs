using UnityEngine;

public class AttackEntity : Entity
{
    public void AttackBorn(
        WeaponResource weaponResource,
        Vector2 position,
        Vector2 direction,
        GameObject bornRoot,
        Vector2? scale = null,
        bool flipY = false,
        bool isDash = false)
    {
        EntityBorn(position, direction, bornRoot, scale, flipY);

        if (animator != null && weaponResource != null) // base.animator
        {
            animator.SetInteger("AttackIndex", (int)weaponResource.AttackType);
            animator.SetBool("IsDash", isDash);
            animator.SetTrigger("Attack");
        }
    }
}
