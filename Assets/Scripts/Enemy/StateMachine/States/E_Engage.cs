using UnityEngine;

public class E_Engage : EnemyState
{
    [Header("移动")]
    [SerializeField] private float engageSpeed = 4f;

    [Header("距离")]
    [SerializeField] private float preferredDistance = 2f;
    [SerializeField] private float attackDistanceOffset = 1f;

    [Header("追击")]
    [SerializeField] private float approachWeight = 2.5f;

    [Header("绕行")]
    [SerializeField] private float orbitWeight = 0.6f;
    [SerializeField] private float orbitChangeTime = 1.5f;

    [Header("攻击压迫")]
    [SerializeField] private float closePressure = 1.5f;

    [Header("墙壁")]
    [SerializeField] private float wallDetectDistance = 1f;
    [SerializeField] private float wallAvoidWeight = 2f;

    [Header("转向")]
    [SerializeField] private float directionSmooth = 12f;


    private float orbitTimer;
    private int orbitDirection;
    private float targetDistance;


    public override void StateEnter()
    {
        orbitDirection = enemy.GetInstanceID() % 2 == 0 ? 1 : -1;

        targetDistance = preferredDistance + Random.Range(-attackDistanceOffset, attackDistanceOffset);

        orbitTimer = Random.Range(0.5f, orbitChangeTime);
    }


    public override void StateUpdate()
    {
        if (enemy.Player == null)
            return;


        Vector2 toPlayer = enemy.Player.position - enemy.transform.position;
        float distance = toPlayer.magnitude;
        Vector2 playerDir = toPlayer.normalized;


        enemy.faceDir = playerDir;


        if (!HasLineOfSight())
        {
            stateMachine.ChangeToState("Follow");
            return;
        }


        if (enemy.IsPlayerInAttackRange && !enemy.IsAttackColdDown)
        {
            stateMachine.ChangeToState("Attack");
            return;
        }


        orbitTimer -= Time.deltaTime;

        if (orbitTimer <= 0)
        {
            orbitTimer = Random.Range(orbitChangeTime * 0.5f, orbitChangeTime * 1.5f);

            if (Random.value < 0.4f)
                orbitDirection *= -1;
        }


        Vector2 move = Vector2.zero;


        // ==========================
        // 距离压力
        // ==========================

        float distanceError = distance - targetDistance;

        float pressure = Mathf.Clamp(distanceError / preferredDistance, -1f, 1f);


        // 永远存在追击压力
        move += playerDir * pressure * approachWeight;



        // ==========================
        // 中近距离增加攻击欲望
        // ==========================

        if (distance < preferredDistance + 1f)
        {
            move += playerDir * closePressure;
        }



        // ==========================
        // 轻微侧移
        // ==========================

        if (distance < preferredDistance + 1.5f)
        {
            Vector2 orbit = Vector2.Perpendicular(playerDir) * orbitDirection;

            move += orbit * orbitWeight;
        }



        // 随机扰动
        move += Random.insideUnitCircle * 0.08f;



        // 墙壁处理
        move += CalculateWallAvoid() * wallAvoidWeight;



        if (move.sqrMagnitude > 0.01f)
        {
            Vector2 targetDir = move.normalized;

            enemy.moveDir = Vector2.Lerp(enemy.moveDir, targetDir, Time.deltaTime * directionSmooth);
        }
    }



    private Vector2 CalculateWallAvoid()
    {
        Vector2 dir = enemy.moveDir;

        if (dir.sqrMagnitude < 0.01f)
            dir = enemy.faceDir;


        RaycastHit2D hit = Physics2D.Raycast(enemy.transform.position, dir, wallDetectDistance, LayerMask.GetMask("Wall"));


        if (hit.collider != null)
        {
            return hit.normal;
        }


        return Vector2.zero;
    }



    private bool HasLineOfSight()
    {
        Vector2 dir = enemy.Player.position - enemy.transform.position;


        RaycastHit2D hit = Physics2D.Raycast(enemy.transform.position, dir.normalized, dir.magnitude, LayerMask.GetMask("Wall"));


        return hit.collider == null;
    }



    public override void StateFixedUpdate()
    {
        Vector2 delta = enemy.moveDir * engageSpeed * Time.fixedDeltaTime;


        if (enemy.Rigidbody2D != null)
        {
            enemy.Rigidbody2D.MovePosition(enemy.Rigidbody2D.position + delta);
        }
        else
        {
            enemy.transform.Translate(delta, Space.World);
        }
    }
}