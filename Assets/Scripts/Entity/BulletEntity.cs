using UnityEngine;

public class BulletEntity : Entity
{
    [Header("子弹参数")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifeTime = 5f;

    [Header("碰撞体（IsTrigger = true）")]
    [SerializeField] private Collider2D bulletCollider;

    public GameObject owner;

    private Vector2 moveDirection;
    private float lifeTimer;


    public override void EntityBorn(Vector2 position, Vector2 direction, GameObject bornRoot, Vector2? scale = null, bool flipY = false, System.Action onDestroy = null)
    {
        base.EntityBorn(position, direction, bornRoot, scale, flipY, onDestroy);
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
        EntityDestroy();
    }

    public override void EntityDestroy()
    {
        base.EntityDestroy(); 
    }
}
