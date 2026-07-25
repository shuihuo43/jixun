using UnityEngine;

public class WraithEntity : Entity
{
    [Header("移动")]
    [SerializeField] private float speed = 6f;
    [SerializeField] private float switchInterval = 1.5f;

    [Header("追踪")]
    [SerializeField] private float searchRange = 10f;
    [SerializeField] private float turnRate = 2f;

    [Header("生命周期")]
    [SerializeField] private float lifetime = 4f;

    private Transform target;
    private Vector2 velocity;
    private float timer;
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

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime) { EntityDestroy(); return; }

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
}
