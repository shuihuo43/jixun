using UnityEngine;

public class WraithEntity : ElementEntity
{
    [Header("移动")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float switchInterval = 1.5f;

    [Header("追踪")]
    [SerializeField] private float searchRange = 10f;
    [SerializeField] private float turnRate = 2f;

    private Transform target;
    private Vector2 velocity;
    private float switchTimer;
    private Vector2 moveDir;

    public override void EntityBorn(Vector2 pos, Vector2 dir, GameObject root, Vector2? scale = null, bool flipY = false, System.Action onDestroy = null, GameObject ownerObj = null)
    {
        moveDir = dir.normalized;
        transform.SetParent(root.transform, false);
        transform.localPosition = pos;
        transform.localRotation = Quaternion.identity;

        if (bornClip != null)
            AudioManager.Instance?.PlaySFX(bornClip, bornVolume);
    }

    void Start()
    {
        float baseAngle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        float angle = baseAngle + Random.Range(-90f, 90f);
        velocity = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)) * speed;
        PickRandomTarget();
    }

    protected override void Update()
    {
        base.Update();

        if (target == null || !IsAlive(target))
            PickRandomTarget();

        switchTimer -= Time.deltaTime;
        if (switchTimer <= 0f)
            PickRandomTarget();

        if (target != null)
        {
            Vector2 toTarget = target.position - transform.position;
            Vector2 desired = toTarget.normalized * speed;
            velocity = Vector2.MoveTowards(velocity, desired, turnRate * Time.deltaTime * speed);
        }

        transform.Translate(velocity * Time.deltaTime, Space.World);
    }

    bool IsAlive(Transform t)
    {
        var en = t.GetComponent<Enemy>();
        return en != null && en.resource != null && en.resource.currentHealth > 0f;
    }

    void PickRandomTarget()
    {
        switchTimer = switchInterval + Random.Range(-0.3f, 0.3f);

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        var inRange = new System.Collections.Generic.List<Transform>();
        foreach (var e in enemies)
        {
            if (!IsAlive(e.transform)) continue;
            if (Vector2.Distance(transform.position, e.transform.position) <= searchRange)
                inRange.Add(e.transform);
        }

        target = inRange.Count > 0 ? inRange[Random.Range(0, inRange.Count)] : null;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;
        if (age < 0.1f) return;

        var enemy = other.GetComponent<Enemy>();
        var pr = GameManager.Instance?.player?.PlayerResource;
        if (pr != null && pr.HasBoolBuff(PlayerResource.BoolBuffType.GhostSpawnWraithOnMark))
        {
            if (enemy?.resource != null && enemy.resource.statusDict.TryGetValue(DebuffType.Blood, out int blood) && blood > 0)
            {
                enemy.resource.statusDict[DebuffType.Blood] = blood - 1;
                var prefab = GameManager.Instance?.GetEntity("恶灵");
                if (prefab != null)
                {
                    int count = pr.ghostSpawnCount;
                    for (int i = 0; i < count; i++)
                    {
                        var obj = Instantiate(prefab, transform.position, Quaternion.identity);
                        var ent = obj.GetComponent<Entity>();
                        var root = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
                        if (root == null) root = other.gameObject;
                        if (ent != null) ent.EntityBorn(root.transform.InverseTransformPoint(transform.position), Random.insideUnitCircle.normalized, root, ownerObj: gameObject);
                    }
                }
            }
        }

        var persist = pr?.HasBoolBuff(PlayerResource.BoolBuffType.GhostPersistOnHit) ?? false;
        Debug.Log($"[WraithEntity] persist={persist}");
        if (!persist)
            EntityDestroy();
    }
}
