using System;
using UnityEngine;

/// <summary>
/// 实体基类：只负责位置、朝向、缩放
/// </summary>
public class Entity : MonoBehaviour
{
    private Action onDestroyCallback;

    public virtual void EntityBorn(
        Vector2 position,
        Vector2 direction,
        GameObject bornRoot,
        Vector2? scale = null,
        bool flipY = false,
        Action onDestroy = null)
    {
        transform.SetParent(bornRoot.transform);
        transform.localPosition = position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        Vector2 finalScale = scale ?? Vector2.one;
        if (flipY) finalScale.y *= -1f;
        transform.localScale = new Vector3(finalScale.x, finalScale.y, 1f);

        onDestroyCallback = onDestroy;
    }

    public virtual void EntityDestroy()
    {
        onDestroyCallback?.Invoke();
        Destroy(gameObject);
    }
}
