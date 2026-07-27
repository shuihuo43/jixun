using UnityEngine;

public class E_Attack : EnemyState
{
    public GameObject attackPrefab;
    public float windup = 0.25f;
    [SerializeField] private float predictionTime = 0.3f;

    private float timer;
    private bool fired;
    private Vector2 aimDir;

    public override void StateEnter()
    {
        timer = windup;
        fired = false;
        enemy.moveDir = Vector2.zero;

        // 预判瞄准
        Vector3 predicted = enemy.Player.position;
        var p = enemy.Player.GetComponent<Player>();
        if (p != null && p.MoveInput.sqrMagnitude > 0.01f)
            predicted += (Vector3)(p.MoveInput * p.MoveSpeed * predictionTime);
        aimDir = (predicted - enemy.transform.position).normalized;

        // 前摇期间转向预判方向
        enemy.faceDir = aimDir;
    }

    public override void StateUpdate()
    {
        if (enemy.Player == null) return;

        timer -= Time.deltaTime;

        if (timer <= 0 && !fired)
        {
            fired = true;
            OnAttack();
        }
    }

    protected virtual void OnAttack()
    {
        enemy.StartAttackCooldown();

        GameObject obj = Instantiate(attackPrefab, enemy.transform.position, Quaternion.identity);
        SectorRange range = obj.GetComponent<SectorRange>();
        if (range)
        {
            range.EntityBorn(Vector2.zero, aimDir, enemy.gameObject, onDestroy: OnAttackEnd);
            return;
        }

        OnAttackEnd();
    }

    protected virtual void OnAttackEnd()
    {
        stateMachine.ChangeToState(enemy.EngageStateName);
    }
}