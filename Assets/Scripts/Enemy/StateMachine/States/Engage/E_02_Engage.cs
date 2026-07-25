using UnityEngine;

public class E_2_Engage : EnemyState
{
    [Header("移动")]
    [SerializeField] float engageSpeed = 4f;
    [SerializeField] float directionSmooth = 8f;


    [Header("环绕")]
    [SerializeField] float orbitWeight = 3f;
    [SerializeField] float orbitRadius = 3f;


    [Header("群体站位")]
    [SerializeField] float enemyDetectRadius = 6f;
    [SerializeField] float crowdWeight = 2f;
    [SerializeField] LayerMask enemyLayer;


    [Header("墙壁")]
    [SerializeField] float wallDetectDistance = 1f;
    [SerializeField] float wallWeight = 3f;


    [Header("随机")]
    [SerializeField] float randomWeight = 0.3f;


    Vector2 orbitDir;



    public override void StateUpdate()
    {
        if (enemy.Player == null)
            return;


        Vector2 playerDir =
            enemy.transform.position -
            enemy.Player.position;


        enemy.faceDir =
            -playerDir.normalized;



        if (!HasLineOfSight())
        {
            stateMachine.ChangeToState("Follow");
            return;
        }



        if (enemy.IsPlayerInAttackRange &&
           !enemy.IsAttackColdDown)
        {
            stateMachine.ChangeToState("Attack");
            return;
        }



        Vector2 move = Vector2.zero;



        // 环绕玩家
        move += GetOrbitDirection()
                * orbitWeight;



        // 根据敌人分布调整方向
        move += CrowdBalance()
                * crowdWeight;



        // 墙壁
        move += WallAvoid()
                * wallWeight;



        // 微小随机
        move += Random.insideUnitCircle
                * randomWeight;



        if (move.sqrMagnitude > 0.01f)
        {
            enemy.moveDir =
                Vector2.Lerp(
                    enemy.moveDir,
                    move.normalized,
                    Time.deltaTime *
                    directionSmooth);
        }
    }



    Vector2 GetOrbitDirection()
    {
        Vector2 dir =
            (enemy.transform.position -
             enemy.Player.position)
             .normalized;


        // 切线方向

        int sign =
            enemy.GetInstanceID() % 2 == 0
            ? 1 : -1;


        return
            Vector2.Perpendicular(dir)
            * sign;
    }



    Vector2 CrowdBalance()
    {
        Collider2D[] enemies =
            Physics2D.OverlapCircleAll(
                enemy.Player.position,
                enemyDetectRadius,
                enemyLayer);



        int left = 0;
        int right = 0;


        Vector2 selfDir =
            enemy.transform.position -
            enemy.Player.position;



        foreach (Collider2D col in enemies)
        {
            if (col.gameObject == enemy.gameObject)
                continue;


            Vector2 otherDir =
                col.transform.position -
                enemy.Player.position;



            float cross =
                Cross2D(
                    selfDir,
                    otherDir);



            if (cross > 0)
                left++;
            else
                right++;
        }

        float Cross2D(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        if (left > right)
        {
            return
                -Vector2.Perpendicular(
                    selfDir.normalized);
        }


        if (right > left)
        {
            return
                Vector2.Perpendicular(
                    selfDir.normalized);
        }


        return Vector2.zero;
    }



    Vector2 WallAvoid()
    {
        Vector2 dir =
            enemy.moveDir;


        if (dir.sqrMagnitude < 0.01f)
            dir = enemy.faceDir;



        RaycastHit2D hit =
            Physics2D.Raycast(
                enemy.transform.position,
                dir,
                wallDetectDistance,
                LayerMask.GetMask("Wall"));


        if (hit.collider)
            return hit.normal;


        return Vector2.zero;
    }



    bool HasLineOfSight()
    {
        Vector2 dir =
            enemy.Player.position -
            enemy.transform.position;


        return Physics2D.Raycast(
            enemy.transform.position,
            dir.normalized,
            dir.magnitude,
            LayerMask.GetMask("Wall"))
            .collider == null;
    }



    public override void StateFixedUpdate()
    {
        Vector2 delta =
            enemy.moveDir *
            engageSpeed *
            Time.fixedDeltaTime;


        if (enemy.Rigidbody2D)
        {
            enemy.Rigidbody2D.MovePosition(
                enemy.Rigidbody2D.position +
                delta);
        }
        else
        {
            enemy.transform.Translate(
                delta,
                Space.World);
        }
    }
}