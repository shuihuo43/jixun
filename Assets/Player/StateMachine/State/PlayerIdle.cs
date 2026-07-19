using UnityEngine;

public class PlayerIdle : PlayerState
{
    public PlayerIdle(string name, Player player, PlayerStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (player.MoveInput != Vector2.zero)
        {
            stateMachine.ChangeToState("Move");
        }
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();
        player.ApplyDeceleration();
    }
}
