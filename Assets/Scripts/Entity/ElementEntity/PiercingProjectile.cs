using UnityEngine;

/// <summary>
/// 可贯穿/附着敌人的弹射物基类：追踪、插在敌人身上、冲刺召回
/// </summary>
public abstract class PiercingProjectile : ElementEntity
{
    [Header("追踪")]
    [SerializeField] protected float speed = 12f;
    [SerializeField] protected float turnRate = 10f;
    [SerializeField] protected float searchRange = 12f;
    [SerializeField] protected float attachRange = 0.5f;

    [Header("碰撞")]
    [SerializeField] protected Collider2D projectileCollider;

    [Header("召回")]
    [SerializeField] protected float recallSpeed = 20f;

    protected Vector2 velocity;
    protected Transform target;
    protected bool stuck;
    protected Transform attachedEnemy;
    protected bool recalling;

    public bool IsAttached => attachedEnemy != null;

    public override void EntityBorn(Vector2 pos, Vector2 dir, GameObject root, Vector2? scale = null, bool flipY = false, System.Action onDestroy = null, GameObject ownerObj = null)
    {
        transform.SetParent(root != null ? root.transform : null, false);
        transform.localPosition = pos;
        velocity = dir.normalized * speed;

        target = FindNearestEnemy();
        if (target != null)
            velocity = ((Vector2)target.position - (Vector2)transform.position).normalized * speed;

        if (bornClip != null) AudioManager.Instance?.PlaySFX(bornClip, bornVolume);
        OnBorn();
    }

    /// <summary>子类额外初始化</summary>
    protected virtual void OnBorn() { }

    Transform FindNearestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float nearest = searchRange;
        Transform best = null;
        foreach (var e in enemies)
        {
            var en = e.GetComponent<Enemy>();
            if (en == null || (en.resource != null && en.resource.currentHealth <= 0f)) continue;
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < nearest) { nearest = d; best = e.transform; }
        }
        return best;
    }

    /// <summary>冲刺召回：飞向玩家并自毁</summary>
    public virtual void Recall(Transform player, float spd)
    {
        recallSpeed = spd;
        attachedEnemy = null;
        stuck = false;
        if (projectileCollider != null) projectileCollider.enabled = true;
        target = player;
        recalling = true;
    }

    protected override void Update()
    {
        if (!recalling) base.Update();
        if (recalling)
        {
            if (target == null) { EntityDestroy(); return; }
            Vector2 toTarget = target.position - transform.position;
            if (toTarget.magnitude < 0.5f) { EntityDestroy(); return; }
            velocity = toTarget.normalized * recallSpeed;
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
            transform.Translate(velocity * Time.deltaTime, Space.World);
            return;
        }

        if (attachedEnemy != null)
        {
            if (!IsAlive(attachedEnemy)) { EntityDestroy(); return; }
            transform.position = attachedEnemy.position;
            return;
        }

        if (stuck) return;

        if (target != null && !IsAlive(target)) target = null;

        if (target != null)
        {
            Vector2 toTarget = target.position - transform.position;
            float dist = toTarget.magnitude;

            if (dist < attachRange)
            {
                AttachTo(target, target.position);
                return;
            }

            Vector2 desired = toTarget.normalized * speed;
            velocity = Vector2.MoveTowards(velocity, desired, turnRate * speed * Time.deltaTime);
        }

        Vector2 delta = velocity * Time.deltaTime;
        var hit = Physics2D.Raycast(transform.position, velocity.normalized, delta.magnitude, LayerMask.GetMask("Wall"));
        if (hit.collider != null)
        {
            transform.position = hit.point;
            stuck = true;
        }
        else
        {
            transform.Translate(delta, Space.World);
        }

        if (velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (attachedEnemy != null) return;
        var en = other.GetComponent<Enemy>();
        if (en == null || (en.resource != null && en.resource.currentHealth <= 0f)) return;
        AttachTo(other.transform, other.ClosestPoint(transform.position));
    }

    protected virtual void AttachTo(Transform t, Vector2 attachPoint)
    {
        attachedEnemy = t;
        transform.position = attachPoint;
        if (projectileCollider != null) projectileCollider.enabled = false;
        stuck = false;
    }

    protected bool IsAlive(Transform t)
    {
        var en = t.GetComponent<Enemy>();
        return en != null && (en.resource == null || en.resource.currentHealth > 0f);
    }
}
