using UnityEngine;

/// <summary>冤魂</summary>
public class GhostEntity : ElementEntity
{
    [Header("移动")]
    [SerializeField] private float speed = 8f;
    [SerializeField] private float searchRange = 15f;

    [Header("追踪")]
    [SerializeField] private float homingRange = 8f;
    [SerializeField] private float turnRate = 3f;

    private Transform target;
    private Vector2 velocity;
    private Vector2 moveDir;

    public override void EntityBorn(Vector2 pos, Vector2 dir, GameObject root, Vector2? scale = null, bool flipY = false, System.Action onDestroy = null, GameObject ownerObj = null)
    {
        moveDir = dir.normalized;
        transform.SetParent(root.transform, false);
        transform.localPosition = pos;
        transform.localRotation = Quaternion.identity;
        if (bornClip != null) AudioManager.Instance?.PlaySFX(bornClip, bornVolume);
    }

    void Start()
    {
        float baseAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        float angle = baseAngle + Random.Range(-90f, 90f);
        velocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * speed;
    }

    protected override void Update()
    {
        base.Update();
        if (target == null || !IsAlive(target)) FindTarget();
        if (target != null)
        {
            Vector2 toTarget = target.position - transform.position;
            float dist = toTarget.magnitude;
            if (dist < homingRange)
            {
                Vector2 tangent = new Vector2(-toTarget.y, toTarget.x).normalized;
                Vector2 desired = (toTarget.normalized + tangent * Random.Range(-0.4f, 0.4f)).normalized * speed;
                velocity = Vector2.MoveTowards(velocity, desired, turnRate * Time.deltaTime * speed);
            }
            else target = null;
        }
        transform.Translate(velocity * Time.deltaTime, Space.World);
    }

    bool IsAlive(Transform t) { var e = t.GetComponent<Enemy>(); return e != null && e.resource != null && e.resource.currentHealth > 0f; }

    void FindTarget()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float nearest = searchRange;
        foreach (var e in enemies)
        {
            if (!IsAlive(e.transform)) continue;
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < nearest) { nearest = d; target = e.transform; }
        }
    }
}
