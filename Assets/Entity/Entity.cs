using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] Animator animator;



    public void EntityBorn(
        Vector2 position, Vector2 direction, GameObject bornRoot, Vector2? scale = null, bool flipY = false
        )
    {
        // 挂到指定父对象下
        transform.SetParent(bornRoot.transform);

        // 设置本地位置
        transform.localPosition = position;

        // 计算方向角度（素材默认朝右，即 Vector2.right）
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 设置缩放
        Vector2 finalScale = scale ?? Vector2.one;
        if (flipY)
            finalScale.y *= -1f;
        transform.localScale = new Vector3(finalScale.x, finalScale.y, 1f);
    }

    /// <summary>
    /// 直接销毁自身
    /// </summary>
    public void EntityDestroy()
    {
        Destroy(gameObject);
    }
}
