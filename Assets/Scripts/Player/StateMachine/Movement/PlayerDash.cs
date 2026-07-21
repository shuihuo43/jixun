using UnityEngine;

public class PlayerDash : PlayerState
{
    private Vector2 dashDirection;
    private float dashTimer;

    public PlayerDash(string name, Player player, PlayerStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }

    public override void StateEnter()
    {
        base.StateEnter();

        dashDirection = player.MoveInput != Vector2.zero
            ? player.MoveInput
            : player.PreMovementNotZero;

        dashTimer = player.DashDuration;
        player.IsDashing = true;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0f)
        {
            EndDash();
        }
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();
        player.transform.Translate(dashDirection * player.DashSpeed * Time.fixedDeltaTime);
    }

    public override void StateExit()
    {
        base.StateExit();
        player.IsDashing = false;
        dashTimer = -1f;
    }

    private void EndDash()
    {
        // 冲刺结束保留速度
        player.SetDashEndVelocity(dashDirection);

        if (Input.GetKey(KeyCode.Space))
        {
            stateMachine.ChangeToState("Run");
        }
        else
        {
            stateMachine.ChangeToState("Move");
        }
    }
}
