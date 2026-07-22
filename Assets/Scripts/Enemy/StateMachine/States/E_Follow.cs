using System.Collections.Generic;
using UnityEngine;

/// <summary>追踪：连线有墙 → A* 寻路。连线通畅 → 切 Engage</summary>
public class E_Follow : EnemyState
{
    [SerializeField] private float followSpeed = 4f;
    private List<Vector3Int> path;

    public override void StateUpdate()
    {
        if (enemy.Player == null) return;

        // 连线通畅 → 交战
        if (HasLineOfSight())
        {
            stateMachine.ChangeToState("Engage");
            return;
        }

        // 不通 → A* 寻路
        if (enemy.pathfinding != null)
            path = enemy.pathfinding.FindPath(enemy.transform.position, enemy.Player.position);

        if (path != null && path.Count > 1)
            enemy.moveDir = (enemy.pathfinding.CellToWorld(path[1]) - (Vector2)enemy.transform.position).normalized;
        else
            enemy.moveDir = (enemy.Player.position - enemy.transform.position).normalized;

        enemy.faceDir = enemy.moveDir;
    }

    bool HasLineOfSight()
    {
        Vector2 toPlayer = enemy.Player.position - enemy.transform.position;
        var hit = Physics2D.Raycast(enemy.transform.position, toPlayer.normalized, toPlayer.magnitude, LayerMask.GetMask("Wall"));
        return hit.collider == null;
    }

    public override void StateFixedUpdate()
    {
        Vector2 delta = enemy.moveDir * followSpeed * Time.fixedDeltaTime;
        if (enemy.Rigidbody2D != null)
            enemy.Rigidbody2D.MovePosition(enemy.Rigidbody2D.position + delta);
        else
            enemy.transform.Translate(delta, Space.World);
    }
}
