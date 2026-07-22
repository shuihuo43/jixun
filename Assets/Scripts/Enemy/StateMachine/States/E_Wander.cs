using UnityEngine;

/// <summary>游走：停顿 → 随机方向移动 → 停顿 循环</summary>
public class E_Wander : EnemyState
{
    [SerializeField] private float wanderSpeed = 1.5f;
    [SerializeField] private float pauseDuration = 0.5f;
    [SerializeField] private float moveDuration = 1.5f;
    [SerializeField] private float randomOffset = 0.3f;

    private bool moving;
    private float timer;

    public override void StateEnter()
    {
        moving = false;
        timer = pauseDuration + Random.Range(-randomOffset, randomOffset);
        enemy.moveDir = Vector2.zero;
    }

    public override void StateUpdate()
    {
        if (enemy.IsPlayerDetected)
        {
            stateMachine.ChangeToState("Follow");
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            moving = !moving;
            if (moving)
            {
                timer = moveDuration + Random.Range(-randomOffset, randomOffset);
                float a = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                enemy.moveDir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                enemy.faceDir = enemy.moveDir;
            }
            else
            {
                timer = pauseDuration + Random.Range(-randomOffset, randomOffset);
                enemy.moveDir = Vector2.zero;
            }
        }
    }

    public override void StateFixedUpdate()
    {
        if (!moving) return;
        Vector2 delta = enemy.moveDir * wanderSpeed * Time.fixedDeltaTime;
        if (enemy.Rigidbody2D != null)
            enemy.Rigidbody2D.MovePosition(enemy.Rigidbody2D.position + delta);
        else
            enemy.transform.Translate(delta, Space.World);
    }
}
