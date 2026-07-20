using UnityEngine;

public class ActionAttack : ActionState
{
    private float attackTimer;
    private int totalHits;      // 本次攻击总段数
    private int nextHitIndex;   // 下一段索引（0-based）
    private float hitInterval;  // 段间隔
    private float hitTimer;     // 下一段倒计时

    public ActionAttack(string name, Player player, ActionStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }

    public override void StateEnter()
    {
        base.StateEnter();

        player.IsAttacking = true;
        player.IsSlowMove = true;

        totalHits = player.CurWeapon != null ? player.CurWeapon.AttackCount : 1;
        attackTimer = player.AttackDuration;

        // 第一段立刻触发
        player.AttackLogic();
        nextHitIndex = 1;

        // 连击间隔: 基础时长 / 段数
        if (totalHits > 1)
        {
            hitInterval = player.AttackBaseDuration / totalHits;
            hitTimer = hitInterval;
        }
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        attackTimer -= Time.deltaTime;

        // 多段攻击触发
        if (nextHitIndex < totalHits)
        {
            hitTimer -= Time.deltaTime;
            if (hitTimer <= 0f)
            {
                player.AttackLogic();
                nextHitIndex++;

                if (nextHitIndex < totalHits)
                    hitTimer = hitInterval;
            }
        }

        if (attackTimer <= 0f)
        {
            stateMachine.ChangeToState("None");
        }
    }

    public override void StateExit()
    {
        base.StateExit();
        player.IsAttacking = false;
        player.IsSlowMove = false;
        attackTimer = -1f;
        totalHits = 0;
        nextHitIndex = 0;
    }
}
