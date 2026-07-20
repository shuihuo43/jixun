using UnityEngine;

public class PlayerRun : PlayerState
{
    public PlayerRun(string name, Player player, PlayerStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }

    public override void StateEnter()
    {
        base.StateEnter();
        player.IsRunning = true;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        // 攻击期间锁定状态，攻击结束再判断
        if (player.IsAttacking) return;

        // 松空格 → 回到移动
        if (!Input.GetKey(KeyCode.Space))
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
        player.ApplyMovement(player.RunSpeed);
    }

    public override void StateExit()
    {
        base.StateExit();
        player.IsRunning = false;
    }
}
