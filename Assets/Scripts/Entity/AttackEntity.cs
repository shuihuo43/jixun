using UnityEngine;

public class AttackEntity : Entity
{
    [SerializeField] private SpriteRenderer weaponSprite;

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

        if (animator != null && weaponResource != null)
        {
            animator.SetInteger("AttackIndex", (int)weaponResource.AttackType);
            animator.SetBool("IsDash", isDash);
            animator.SetTrigger("Attack");
        }

        if (weaponResource != null && weaponSprite != null)
            weaponSprite.sprite = weaponResource.icon;

        // 描边 + 染色材质
        if (weaponSprite != null && damageResource != null && DamageNumberCanvas.Instance != null)
        {
            var mat = new Material(Shader.Find("Custom/SpriteOutline"));
            mat.mainTexture = weaponSprite.sprite?.texture;
            mat.SetColor("_Color", DamageNumberCanvas.Instance.GetColor(damageResource.damageType));
            mat.SetColor("_OutlineColor", Color.white);
            mat.SetFloat("_OutlineWidth", 0.005f);
            weaponSprite.material = mat;
        }
    }

}
