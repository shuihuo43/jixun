using UnityEngine;

public class E_Attack : EnemyState
{
    public GameObject attackPrefab;
    public float windup = 0.25f;

    private float timer;
    private bool fired;

    public override void StateEnter()
    {
        timer = windup;
        fired = false;
        enemy.moveDir = Vector2.zero;
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
            range.EntityBorn(Vector2.zero, enemy.faceDir, enemy.gameObject, onDestroy: OnAttackEnd);
            return;
        }

        OnAttackEnd();
    }

    protected virtual void OnAttackEnd()
    {
        stateMachine.ChangeToState("Engage");
    }
}