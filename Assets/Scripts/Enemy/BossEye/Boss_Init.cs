using UnityEngine;

public class Boss_Init : EnemyState
{
    bool triggered;

    public override void StateEnter()
    {
        enemy.moveDir = Vector2.zero;
        triggered = false;
    }

    public override void StateUpdate()
    {
        var boss = enemy as Boss_Eye;
        if (boss == null || enemy.Player == null) return;
        if (triggered) return;

        bool inRange;

        if (boss.triggerCollider != null)
        {
            inRange = boss.triggerCollider.OverlapPoint(enemy.Player.position);
        }
        else
        {
            // 退路：直接按距离检测
            float dist = Vector2.Distance(enemy.transform.position, enemy.Player.position);
            inRange = dist < 8f;
        }

        if (!triggered && inRange)
            Debug.Log($"[Boss_Init] triggerCollider={boss.triggerCollider?.name ?? "NULL"}, dist={Vector2.Distance(enemy.transform.position, enemy.Player.position):F1}, inRange={inRange}");

        if (inRange)
        {
            triggered = true;
            Debug.Log("[Boss_Init] Player detected, switching to Follow");
            stateMachine.ChangeToState("Follow");
        }
    }
}
