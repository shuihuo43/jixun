using UnityEngine;

public class Boss_DashAttack : EnemyState
{
    [SerializeField] private float windupTime = 0.2f;
    [SerializeField] private GameObject entityPrefab;
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float predictionTime = 0.3f;

    private Vector2 dashDir;
    private float timer;
    private bool fired;
    private bool dashing;

    public override void StateEnter()
    {
        timer = windupTime;
        fired = false;
        dashing = false;
        enemy.moveDir = Vector2.zero;
        var boss = enemy as Boss_Eye;
        if (boss != null) boss.dashCount++;
        if (enemy.Player != null)
        {
            Vector3 predicted = enemy.Player.position;
            var p = enemy.Player.GetComponent<Player>();
            if (p != null && p.MoveInput.sqrMagnitude > 0.01f)
                predicted += (Vector3)(p.MoveInput * p.MoveSpeed * predictionTime);
            dashDir = (predicted - enemy.transform.position).normalized;
        }
        else
        {
            dashDir = enemy.faceDir;
        }
    }

    public override void StateUpdate()
    {
        if (!dashing)
        {
            enemy.faceDir = dashDir;
            timer -= Time.deltaTime;
            if (timer <= 0f && !fired)
            {
                fired = true;
                SpawnEntity();
            }
        }
    }

    void SpawnEntity()
    {
        if (entityPrefab == null) return;
        var obj = Instantiate(entityPrefab, enemy.transform.position, Quaternion.identity);
        var entity = obj.GetComponent<Entity>();
        if (entity != null)
        {
            var root = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
            if (root == null) root = enemy.gameObject;
            var localPos = root.transform.InverseTransformPoint(enemy.transform.position);
            entity.EntityBorn(localPos, dashDir, root, onDestroy: StartDash);
        }
        else { StartDash(); }
    }

    private float currentSpeed;

    void StartDash()
    {
        dashing = true;
        currentSpeed = dashSpeed;
        timer = dashDuration;
        enemy.moveDir = dashDir;
    }

    public override void StateFixedUpdate()
    {
        if (!dashing) return;

        timer -= Time.fixedDeltaTime;
        if (timer <= 0f)
        {
            var boss = enemy as Boss_Eye;
            float chainChance = 1f - boss.dashCount * 0.2f;
            if (Random.value <= chainChance)
                stateMachine.ChangeToState("DashAttack");
            else
            {
                if (boss != null) boss.dashCount = 0;
                stateMachine.ChangeToState("Rest");
            }
            return;
        }

        currentSpeed *= 0.95f;
        Vector2 delta = dashDir * currentSpeed * Time.fixedDeltaTime;
        if (enemy.Rigidbody2D != null)
            enemy.Rigidbody2D.MovePosition(enemy.Rigidbody2D.position + delta);
    }
}
