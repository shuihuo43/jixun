using UnityEngine;

public class PlayerAttack : PlayerState
{
    private float attackTimer;

    public PlayerAttack(string name, Player player, PlayerStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }

    public override void StateEnter()
    {
        base.StateEnter();

        player.IsAttacking = true;
        player.IsSlowMove = true;
        player.AttackLogic();
        attackTimer = player.AttackDuration;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f)
        {
            // 攻击结束，回到合适的状态
            if (player.MoveInput != Vector2.zero)
                stateMachine.ChangeToState("Move");
            else
                stateMachine.ChangeToState("Idle");
        }
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();
        player.ApplyMovement(player.SlowMoveSpeed);
    }

    public override void StateExit()
    {
        base.StateExit();
        player.IsAttacking = false;
        player.IsSlowMove = false;
        attackTimer = -1f;
    }
}
