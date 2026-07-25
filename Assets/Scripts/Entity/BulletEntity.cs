using UnityEngine;

public class BulletEntity : Entity
{
    [Header("子弹参数")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f;

    [Header("碰撞体（IsTrigger = true）")]
    [SerializeField] private Collider2D bulletCollider;

    private Vector2 moveDirection;
    private float lifeTimer;

    public override void EntityBorn(Vector2 position, Vector2 direction, GameObject bornRoot, Vector2? scale = null, bool flipY = false, System.Action onDestroy = null, GameObject ownerObj = null)
    {
        base.EntityBorn(position, direction, bornRoot, scale, flipY, onDestroy, ownerObj);
        moveDirection = direction;
        lifeTimer = lifeTime;
    }

    void FixedUpdate()
    {
        // 按方向移动
        transform.Translate(moveDirection * speed * Time.fixedDeltaTime, Space.World);

        // 生命周期
        lifeTimer -= Time.fixedDeltaTime;
        if (lifeTimer <= 0f)
            EntityDestroy();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 忽略生成者和同类子弹
        if (owner != null && other.gameObject == owner) return;
        if (other.GetComponent<BulletEntity>() != null) return;

        EntityDestroy();
    }

    public override void EntityDestroy()
    {
        base.EntityDestroy(); 
    }
}
