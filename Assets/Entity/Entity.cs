using UnityEngine;

/// <summary>
/// 实体基类：只负责位置、朝向、缩放
/// </summary>
public class Entity : MonoBehaviour
{
    public virtual void EntityBorn(
        Vector2 position,
        Vector2 direction,
        GameObject bornRoot,
        Vector2? scale = null,
        bool flipY = false)
    {
        // 挂父对象
        transform.SetParent(bornRoot.transform);

        // 本地位置
        transform.localPosition = position;

        // 朝向（素材默认朝右）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 缩放
        Vector2 finalScale = scale ?? Vector2.one;
        if (flipY)
            finalScale.y *= -1f;
        transform.localScale = new Vector3(finalScale.x, finalScale.y, 1f);
    }

    public void EntityDestroy()
    {
        Destroy(gameObject);
    }
}
