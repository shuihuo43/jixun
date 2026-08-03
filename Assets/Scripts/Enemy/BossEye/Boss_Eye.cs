using UnityEngine;

public class Boss_Eye : Enemy
{
    public override string EngageStateName => "Follow";
    public override bool UseHurtState => false;

    [Header("受伤特效")]
    [SerializeField] private GameObject hurtEffectPrefab;
    [SerializeField] private int effectCount = 5;

    /// <summary>连续冲刺计数</summary>
    [HideInInspector] public int dashCount;

    [Header("触发")]
    [SerializeField] public Collider2D triggerCollider;

    [Header("召唤")]
    [SerializeField] public GameObject summonWarningPrefab;
    [SerializeField] public GameObject summonEnemyPrefab;
    /// <summary>普通召唤间隔</summary>
    public float summonInterval = 8f;
    /// <summary>强制召唤间隔（到了不管多少怪都召）</summary>
    public float forceSummonInterval = 20f;
    [HideInInspector] public float lastSummonTime = -99f;
    [HideInInspector] public int activeMinionCount;

    void Start()
    {
        if (resource != null)
        {
            resource.OnHealthChanged += OnDamaged;
            resource.OnDeath += KillAllMinions;
        }
    }

    void KillAllMinions()
    {
        foreach (var m in FindObjectsOfType<EyeMinion>())
        {
            if (m.eyeBoss == this && m.resource != null && m.resource.currentHealth > 0f)
                m.resource.ChangeHealth(-9999f);
        }
    }

    void OnDamaged()
    {
        if (hurtEffectPrefab == null || Player == null) return;
        if (!LastHitFromPlayer) return;

        Vector2 away = ((Vector2)transform.position - (Vector2)Player.position).normalized;
        float baseAngle = Mathf.Atan2(away.y, away.x) * Mathf.Rad2Deg;

        GameObject root = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
        if (root == null) root = gameObject;

        for (int i = 0; i < effectCount; i++)
        {
            float t = effectCount == 1 ? 0f : (float)i / (effectCount - 1);
            float angle = Mathf.Lerp(-90f, 90f, t) + baseAngle;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            var obj = Instantiate(hurtEffectPrefab, transform.position, Quaternion.identity);
            var entity = obj.GetComponent<Entity>();
            if (entity != null)
            {
                Vector2 localPos = root.transform.InverseTransformPoint(transform.position);
                entity.EntityBorn(localPos, dir, root);
            }
        }
    }
}
