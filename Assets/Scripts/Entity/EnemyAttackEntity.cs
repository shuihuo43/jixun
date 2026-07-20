using UnityEngine;

/// <summary>
/// 敌人攻击实体：控制预警显示、前摇计时、攻击区域同步
/// </summary>
public class EnemyAttackEntity : MonoBehaviour
{
    [Header("预警")]
    [SerializeField] private GameObject sectorRangePrefab;
    [SerializeField] private float windupTime = 0.5f;

    [Header("攻击区域（仅调试可视化）")]
    [SerializeField] private ShapeArea shapeArea;

    private SectorRange activeWarning;
    private float windupTimer;
    private bool isWindingUp;

    public float WindupTime => windupTime;
    public bool IsWindingUp => isWindingUp;

    public void Start()
    {
        StartWindup();
    }

    /// <summary>开始攻击前摇，返回风力时间</summary>
    public void StartWindup()
    {
        if (isWindingUp) return;

        isWindingUp = true;
        windupTimer = 0f;

        // 实例化预警
        if (sectorRangePrefab != null)
        {
            GameObject obj = Instantiate(sectorRangePrefab, transform.position, transform.rotation, transform);
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

        if (activeWarning != null)
        {
            activeWarning.DestroyWithFade();
            activeWarning = null;
        }
    }
}
