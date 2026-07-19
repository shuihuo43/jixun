using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("生命值")]
    public int health = 50;

    [Header("移动与追击")]
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float attackRange = 2f;

    [Header("攻击")]
    public float damage = 10f;
    [Range(1f, 360f)] public float attackAngle = 90f;
    public float attackWindup = 0.5f;
    public float attackCooldown = 1f;

    [Header("攻击预警")]
    public GameObject sectorRangePrefab;

    private Transform player;
    private float attackTimer = 0f;

    // 前摇状态
    private bool isWindingUp = false;
    private float windupTimer = 0f;
    private SectorRange activeSectorRange;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogError("没有找到 Player 标签对象！");
    }

    void Update()
    {
        if (player == null) return;

        // 攻击冷却
        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, player.position);

        // 超出仇恨范围 → 停止追击，前摇继续（必定完成）
        if (distance > detectionRange)
            return;

        // 追击（有前摇时也继续追）
        if (distance > attackRange)
        {
            ChasePlayer();
            // 不取消前摇，前摇完成后无论距离都造成伤害
        }

        // 进入攻击范围 → 触发前摇
        if (!isWindingUp && attackTimer <= 0f && distance <= attackRange)
            StartWindup();

        if (isWindingUp)
            UpdateWindup();
    }

    /// <summary>追向玩家</summary>
    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    /// <summary>开始攻击前摇</summary>
    void StartWindup()
    {
        isWindingUp = true;
        windupTimer = 0f;

        if (sectorRangePrefab != null)
        {
            GameObject obj = Instantiate(sectorRangePrefab, transform.position, transform.rotation, transform);
            activeSectorRange = obj.GetComponent<SectorRange>();
            if (activeSectorRange != null)
            {
                activeSectorRange.SectorAngle = attackAngle;
                activeSectorRange.SectorRadius = attackRange;
                activeSectorRange.Process = 0f;
            }
        }
    }

    /// <summary>更新前摇进度，满了必定攻击</summary>
    void UpdateWindup()
    {
        windupTimer += Time.deltaTime;
        float progress = Mathf.Clamp01(windupTimer / attackWindup);

        if (activeSectorRange != null)
            activeSectorRange.Process = progress;

        if (progress >= 1f)
        {
            // 前摇完成，必定造成伤害
            if (player != null)
            {
                Player p = player.GetComponent<Player>();
                p?.TakeDamage(damage);
            }

            ClearWindup();
            attackTimer = attackCooldown;
        }
    }

    void ClearWindup()
    {
        isWindingUp = false;
        windupTimer = 0f;

        if (activeSectorRange != null)
        {
            activeSectorRange.DestroyWithFade();
            activeSectorRange = null;
        }
    }

    /// <summary>敌人受伤</summary>
    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
            Destroy(gameObject);
    }

    /// <summary>Scene 窗口显示范围</summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
