using UnityEngine;

public class PlayerDashAttack : PlayerState
{
    private Vector2 dashDir;
    private float timer;

    public PlayerDashAttack(string name, Player player, PlayerStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }

    public override void StateEnter()
    {
        base.StateEnter();

        player.canBeHurt = false;
        if (player.HurtCollider != null)
            player.HurtCollider.enabled = false;

        Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mouseDir = ((Vector2)mouse - (Vector2)player.transform.position).normalized;

        var gm = GameManager.Instance;
        if (gm != null && gm.panelData != null && gm.panelData.isDashAttackHelp)
        {
            var locked = gm.GetLockedEnemy(player.transform.position, mouseDir, 10f, 120f);
            dashDir = locked != null ? ((Vector2)locked.position - (Vector2)player.transform.position).normalized : mouseDir;
        }
        else
        {
            dashDir = mouseDir;
        }
        timer = player.dashAttackDuration;

        if (player.dashAttackEntityPrefab != null)
        {
            var obj = Object.Instantiate(player.dashAttackEntityPrefab, player.transform.position, Quaternion.identity);
            var atk = obj.GetComponent<AttackEntity>();
            if (atk != null)
                atk.AttackBorn(player.CurWeapon, Vector2.zero, dashDir, player.gameObject, isDash: true);
        }
    }

    public override void StateUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            EndDashAttack();
    }

    public override void StateFixedUpdate()
    {
        Vector2 delta = dashDir * player.dashAttackSpeed * Time.fixedDeltaTime;

        if (player.Rigidbody2D != null)
        {
            var hit = Physics2D.Raycast(player.Rigidbody2D.position, dashDir, delta.magnitude, LayerMask.GetMask("Wall"));
            if (hit.collider != null)
            {
                dashDir = Vector2.Reflect(dashDir, hit.normal);
                delta = dashDir * delta.magnitude;
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
        player.canBeHurt = true;
        if (player.HurtCollider != null)
            player.HurtCollider.enabled = true;
    }

    void EndDashAttack()
    {
        if (Input.GetKey(KeyCode.Space))
            stateMachine.ChangeToState("Run");
        else
            stateMachine.ChangeToState("Move");
    }
}
