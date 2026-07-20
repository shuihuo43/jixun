using UnityEngine;

public class PlayerSlowMove : PlayerState
{
    public PlayerSlowMove(string name, Player player, PlayerStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }

    public override void StateUpdate()
    {
        base.StateUpdate();

        // 攻击结束 → 根据输入切回正常移动
        if (!player.IsAttacking)
        {
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
}
