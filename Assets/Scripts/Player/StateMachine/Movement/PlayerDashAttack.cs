using UnityEngine;

public class PlayerDashAttack : PlayerState
{
    private Vector2 dashDir;
    private float timer;
    private bool hasLocked;
    private bool hitConfirmed;

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

        var locked = UnityEngine.Object.FindObjectOfType<MouseCursor>()?.lockedEnemy;
        hasLocked = locked != null;
        hitConfirmed = false;
        dashDir = locked != null ? ((Vector2)locked.position - (Vector2)player.transform.position).normalized : mouseDir;
        timer = player.dashAttackDuration;

        if (player.recallBladesOnDash)
            foreach (var blade in UnityEngine.Object.FindObjectsOfType<LeafBladeEntity>())
                blade.Recall(player.transform, player.recallBladeSpeed);

        if (player.dashAttackEntityPrefab != null)
        {
            var obj = Object.Instantiate(player.dashAttackEntityPrefab, player.transform.position, Quaternion.identity);
            var atk = obj.GetComponent<AttackEntity>();
            if (atk != null)
            {
                atk.damageResource = player.dashAttackDamageResource;
                atk.AttackBorn(player.CurWeapon, Vector2.zero, dashDir, player.gameObject, isDash: true);
                atk.EntityBorn(Vector2.zero, dashDir, player.gameObject, onDestroy: () => hitConfirmed = true);
            }
        }
    }

    public override void StateUpdate()
    {
        timer -= Time.deltaTime;
        if (hitConfirmed)
            EndDashAttack();
        else if (timer <= 0f)
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
        player.dashAttackCooldownTimer = player.dashAttackCooldown;
        stateMachine.ChangeToState("Run");

        // 长按攻击 → 立即进入普通攻击
        if (Input.GetMouseButton(0))
            player.ExecuteAttack();
    }
}
