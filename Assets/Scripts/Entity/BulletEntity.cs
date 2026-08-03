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
        if (other.GetComponent<BulletEntity>() != null) return;

        bool isEnemyBullet = owner != null && owner.CompareTag("Enemy");
        bool hitEnemy = other.CompareTag("Enemy");
        bool hitPlayer = other.CompareTag("Player");
        bool hitWall = other.gameObject.layer == LayerMask.NameToLayer("Wall");

        // 对立目标 或 墙壁 → 销毁
        if (hitWall) { EntityDestroy(); return; }
        if (isEnemyBullet && hitPlayer) { EntityDestroy(); return; }
        if (!isEnemyBullet && hitEnemy) { EntityDestroy(); return; }
    }

    public override void EntityDestroy()
    {
        base.EntityDestroy(); 
    }
}
