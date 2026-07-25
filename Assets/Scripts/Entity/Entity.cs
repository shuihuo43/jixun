using System;
using UnityEngine;

/// <summary>
/// 实体基类：只负责位置、朝向、缩放
/// </summary>
public class Entity : MonoBehaviour
{
    public DamageResource damageResource;

    [Header("音效")]
    [SerializeField] protected AudioClip bornClip;
    [SerializeField] protected float bornVolume = 0.8f;

    private Action onDestroyCallback;

    /// <summary>递归持有者：生成链最顶端的角色</summary>
    public GameObject owner { get; protected set; }

    public virtual void EntityBorn(
        Vector2 position,
        Vector2 direction,
        GameObject bornRoot,
        Vector2? scale = null,
        bool flipY = false,
        Action onDestroy = null,
        GameObject ownerObj = null)
    {
        transform.SetParent(bornRoot.transform);
        transform.localPosition = position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector2 finalScale = scale ?? Vector2.one;
        if (flipY) finalScale.y *= -1f;
        transform.localScale = new Vector3(finalScale.x, finalScale.y, 1f);

        // owner：传入优先，否则递归继承，否则 bornRoot 本身
        if (ownerObj != null)
            owner = ownerObj;
        else if (bornRoot != null)
        {
            var parentEntity = bornRoot.GetComponent<Entity>();
            owner = parentEntity != null ? parentEntity.owner : bornRoot;
        }

        onDestroyCallback = onDestroy;

        if (bornClip != null)
            AudioManager.Instance?.PlaySFX(bornClip, bornVolume);
    }

    public virtual void EntityDestroy()
    {
        onDestroyCallback?.Invoke();
        Destroy(gameObject);
    }
}
