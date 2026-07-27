using UnityEngine;

public class Boss_Rest : EnemyState
{
    [SerializeField] private float restDuration = 1.5f;
    [SerializeField] private float spinSpeed = 360f;
    [SerializeField] private float driftSpeed = 8f;
    [SerializeField] private float decayRate = 3f;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float blinkSpeed = 20f;

    private float elapsed;
    private Vector2 driftDir;

    public override void StateEnter()
    {
        elapsed = 0f;
        driftDir = enemy.faceDir;
    }

    public override void StateUpdate()
    {
        elapsed += Time.deltaTime;
        float t = elapsed / restDuration;

        if (t >= 1f)
        {
            stateMachine.ChangeToState(enemy.EngageStateName);
            return;
        }

        // 转速和漂移按指数衰减
        float decay = Mathf.Exp(-decayRate * t);
        float spin = spinSpeed * decay;
        float drift = driftSpeed * decay;

        enemy.faceDir = Quaternion.Euler(0, 0, spin * Time.deltaTime) * enemy.faceDir;
        enemy.moveDir = driftDir * drift;

        if (spriteRenderer != null)
        {
            var c = spriteRenderer.color;
            c.a = Mathf.Abs(Mathf.Sin(elapsed * blinkSpeed));
            spriteRenderer.color = c;
        }
    }

    public override void StateExit()
    {
        if (spriteRenderer != null)
        {
            var c = spriteRenderer.color;
            c.a = 1f;
            spriteRenderer.color = c;
        }
    }

    public override void StateFixedUpdate()
    {
        float decay = Mathf.Exp(-decayRate * Mathf.Clamp01(elapsed / restDuration));
        Vector2 delta = driftDir * driftSpeed * decay * Time.fixedDeltaTime;
        if (enemy.Rigidbody2D != null)
            enemy.Rigidbody2D.MovePosition(enemy.Rigidbody2D.position + delta);
    }
}
