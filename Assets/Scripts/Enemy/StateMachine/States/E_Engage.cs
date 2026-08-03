using UnityEngine;

public class E_Engage : EnemyState
{
    [Header("移动")]
    [SerializeField] private float wanderSpeed = 2f;
    [SerializeField] private float dirChangeInterval = 1.5f;

    private Vector2 wanderDir;
    private float wanderTimer;

    public override void StateEnter()
    {
        PickNewDirection();
    }

    public override void StateUpdate()
    {
        if (enemy.Player == null) return;

        if (enemy.IsPlayerInAttackRange && !enemy.IsAttackColdDown)
        {
            stateMachine.ChangeToState("Attack");
            return;
        }

        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
            PickNewDirection();

        enemy.moveDir = wanderDir;
    }

    void PickNewDirection()
    {
        wanderDir = Random.insideUnitCircle.normalized;
        wanderTimer = dirChangeInterval + Random.Range(-0.5f, 0.5f);
    }

    public override void StateFixedUpdate()
    {
        Vector2 delta = enemy.moveDir * wanderSpeed * Time.fixedDeltaTime;
        if (enemy.Rigidbody2D != null)
            enemy.Rigidbody2D.MovePosition(enemy.Rigidbody2D.position + delta);
    }
}
