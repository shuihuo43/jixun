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
        Vector2 delta = dashDirection * player.DashSpeed * Time.fixedDeltaTime;

        if (player.Rigidbody2D != null)
        {
            // 前方碰撞检测，防止高速穿透
            RaycastHit2D[] hits = new RaycastHit2D[1];
            int hitCount = player.Rigidbody2D.Cast(dashDirection, hits, delta.magnitude);
            if (hitCount > 0)
                delta = dashDirection * hits[0].distance;

            player.Rigidbody2D.MovePosition(player.Rigidbody2D.position + delta);
        }
        else
        {
            player.transform.Translate(delta);
        }
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
