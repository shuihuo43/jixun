using UnityEngine;

/// <summary>敌人攻击实体：前摇计时 + CD，预警由 SectorRange 自动推进</summary>
public class EnemyAttackEntity : MonoBehaviour
{
    [Header("预警")]
    [SerializeField] private GameObject sectorRangePrefab;
    [SerializeField] private float windupTime = 0.5f;
    [SerializeField] private float cooldown = 1f;

    [Header("攻击区域")]
    [SerializeField] private ShapeArea shapeArea;

    private bool isWindingUp;
    private float cooldownTimer;

    public bool IsWindingUp => isWindingUp;
    public bool IsAttacking => isWindingUp;
    public float AttackRange => shapeArea != null ? shapeArea.Radius : 0f;

    public void TryAttack()
    {
        if (!isWindingUp && cooldownTimer <= 0f)
            StartWindup();
    }

    public void StartWindup()
    {
        if (isWindingUp) return;
        isWindingUp = true;

        if (sectorRangePrefab != null)
        {
            Quaternion rot = transform.rotation * Quaternion.Euler(0, 0, -90f);
            var obj = Instantiate(sectorRangePrefab, transform.position, rot, transform);
            var sr = obj.GetComponent<SectorRange>();
            if (sr != null)
                sr.StartWindup(shapeArea, windupTime);
        }

        Invoke(nameof(ClearWindup), windupTime);
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    public void ClearWindup()
    {
        isWindingUp = false;
        cooldownTimer = cooldown;
    }
}
