using UnityEngine;

public class ActionAttack : ActionState
{
    private float attackTimer;

    public ActionAttack(string name, Player player, ActionStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }

    public override void StateEnter()
    {
        base.StateEnter();

        player.IsAttacking = true;
        player.IsSlowMove = true;
        player.CanDashCancel = false;
        player.AttackLogic();
        attackTimer = player.AttackDuration;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        attackTimer -= Time.deltaTime;

        // 攻击后摇最后 20% 可被冲刺取消
        if (!player.CanDashCancel && attackTimer <= player.AttackDuration * player.DashCancelRatio)
        {
            player.CanDashCancel = true;
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
        player.CanDashCancel = false;
        attackTimer = -1f;
    }
}
