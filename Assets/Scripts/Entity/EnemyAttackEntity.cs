using UnityEngine;

/// <summary>
/// 敌人攻击实体：控制预警显示、前摇计时、攻击区域同步
/// </summary>
public class EnemyAttackEntity : MonoBehaviour
{
    [Header("预警")]
    [SerializeField] private GameObject sectorRangePrefab;
    [SerializeField] private float windupTime = 0.5f;
    [SerializeField] private float cooldown = 1f;

    [Header("攻击区域（仅调试可视化）")]
    [SerializeField] private ShapeArea shapeArea;

    private SectorRange activeWarning;
    private float windupTimer;
    private float cooldownTimer;
    private bool isWindingUp;

    public float WindupTime => windupTime;
    public bool IsWindingUp => isWindingUp;

    /// <summary>是否正在攻击中（EnemyBrain 读取）</summary>
    public bool IsAttacking => isWindingUp;

    /// <summary>攻击范围（EnemyBrain 读取）</summary>
    public float AttackRange => shapeArea != null ? shapeArea.Radius : 0f;

    /// <summary>EnemyBrain 调用：尝试发起攻击</summary>
    public void TryAttack()
    {
        if (!isWindingUp && cooldownTimer <= 0f)
            StartWindup();
    }

    /// <summary>开始攻击前摇</summary>
    public void StartWindup()
    {
        if (isWindingUp) return;

        isWindingUp = true;
        windupTimer = 0f;

        // 实例化预警
        if (sectorRangePrefab != null)
        {
            // SectorRange mesh 沿 up 开门，enemy 朝向用 right，差 90°
            Quaternion rot = transform.rotation * Quaternion.Euler(0, 0, -90f);
            GameObject obj = Instantiate(sectorRangePrefab, transform.position, rot, transform);
            activeWarning = obj.GetComponent<SectorRange>();
            if (activeWarning != null)
            {
                // 同步 ShapeArea 的区域到预警
                if (shapeArea != null)
                {
                    activeWarning.SyncFromShapeArea(shapeArea);
                }
                activeWarning.Process = 0f;
            }
        }
    }

    void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;

        if (!isWindingUp) return;

        windupTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(windupTimer / windupTime);

        if (activeWarning != null)
            activeWarning.Process = progress;

        if (progress >= 1f)
        {
            ClearWindup();
        }
    }

    public void ClearWindup()
    {
        isWindingUp = false;
        windupTimer = 0f;
        cooldownTimer = cooldown;

        if (activeWarning != null)
        {
            activeWarning.DestroyWithFade();
            activeWarning = null;
        }
    }
}
