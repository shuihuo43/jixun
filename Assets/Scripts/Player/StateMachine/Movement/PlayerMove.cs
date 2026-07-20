using UnityEngine;

public class PlayerMove : PlayerState
{
    public PlayerMove(string name, Player player, PlayerStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }

    public override void StateUpdate()
    {
        base.StateUpdate();

        // 攻击期间锁定状态，攻击结束再判断
        if (!player.IsAttacking && player.MoveInput == Vector2.zero)
        {
            stateMachine.ChangeToState("Idle");
        }
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();
        player.ApplyMovement(player.MoveSpeed);
    }
}
