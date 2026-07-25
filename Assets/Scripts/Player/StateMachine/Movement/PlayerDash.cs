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

        player.cameraController?.Shake(1f, 1f, 0.15f);

        AudioManager.Instance?.PlaySFX(player.dashClip, player.dashVolume);

        if (player.HurtCollider != null)
            player.HurtCollider.enabled = false;
        player.canBeHurt = false;

        dashDirection = player.MoveInput != Vector2.zero
            ? player.MoveInput
            : player.PreMovementNotZero;

        dashTimer = player.DashDuration;
        player.IsDashing = true;

        if (player.Rigidbody2D != null)
            player.Rigidbody2D.velocity = dashDirection * player.DashSpeed;
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
            var filter = new ContactFilter2D { useTriggers = false };
            RaycastHit2D[] hits = new RaycastHit2D[1];
            int hitCount = player.Rigidbody2D.Cast(dashDirection, filter, hits, delta.magnitude);

            if (hitCount > 0 && hits[0].distance > 0.01f)
            {
                float hitDist = hits[0].distance;
                Vector2 wallNormal = hits[0].normal;
                Vector2 toWall = dashDirection * hitDist;
                float remaining = delta.magnitude - hitDist;
                Vector2 tangent = new Vector2(-wallNormal.y, wallNormal.x);
                if (Vector2.Dot(dashDirection, tangent) < 0f) tangent = -tangent;
                delta = toWall + tangent * remaining;
            }

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
        player.dashAttackBuffer = 0.05f;
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
