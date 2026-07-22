using UnityEngine;

/// <summary>范围索敌：玩家进入检测范围后触发攻击</summary>
public class EnemyTargeting : MonoBehaviour, IEnemyTarget
{
    [Header("索敌范围")]
    public float detectionRange = 10f;

    [Header("同步")]
    [SerializeField] private EnemyBrain brain;

    private Transform player;

    public Transform Player => player;
    public bool IsPlayerDetected { get; private set; }
    public Vector2 DirectionToPlayer { get; private set; }
    public float DistanceToPlayer { get; private set; }
    public float DetectionRange { get => detectionRange; set => detectionRange = value; }

    void Start()
    {
        var obj = GameObject.FindGameObjectWithTag("Player");
        if (obj != null)
            player = obj.transform;
    }

    void Update()
    {
        if (player == null)
        {
            IsPlayerDetected = false;
            return;
        }

        Vector2 toPlayer = player.position - transform.position;
        DistanceToPlayer = toPlayer.magnitude;
        IsPlayerDetected = DistanceToPlayer <= detectionRange;
        DirectionToPlayer = DistanceToPlayer > 0.01f ? toPlayer / DistanceToPlayer : Vector2.zero;

        // 玩家进入范围 → 攻击
        if (IsPlayerDetected && brain != null)
            brain.attack.TryAttack();
    }
}
