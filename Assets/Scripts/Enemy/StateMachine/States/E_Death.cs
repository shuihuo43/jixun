using UnityEngine;

public class E_Death : EnemyState
{
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private int effectCount = 12;
    [SerializeField] private float slideSpeed = 12f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector2 slideDir;

    public override void StateEnter()
    {
        slideDir = enemy.Player != null
            ? ((Vector2)enemy.transform.position - (Vector2)enemy.Player.position).normalized
            : Vector2.up;

        enemy.moveDir = Vector2.zero;
        spriteRenderer.color = Color.gray;
        SpawnEffects();
    }

    void SpawnEffects()
    {
        if (deathEffectPrefab == null) return;

        GameObject root = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
        if (root == null) root = gameObject;

        for (int i = 0; i < effectCount; i++)
        {
            float angle = (360f / effectCount) * i * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 pos = (Vector2)enemy.transform.position + dir * 0.5f;
            var obj = Instantiate(deathEffectPrefab, pos, Quaternion.identity);
            var e = obj.GetComponent<Entity>();
            if (e != null)
                e.EntityBorn(root.transform.InverseTransformPoint(pos), dir, root);
        }
    }

    public override void StateFixedUpdate()
    {
        Vector2 delta = slideDir * slideSpeed * Time.fixedDeltaTime;
        slideSpeed *= 0.95f;

        if (enemy.Rigidbody2D != null)
        {
            var hit = Physics2D.Raycast(enemy.Rigidbody2D.position, slideDir, delta.magnitude, LayerMask.GetMask("Wall"));
            if (hit.collider != null)
            {
                slideDir = Vector2.Reflect(slideDir, hit.normal);
                delta = slideDir * delta.magnitude;
            }
            enemy.Rigidbody2D.MovePosition(enemy.Rigidbody2D.position + delta);
        }
        else
        {
            enemy.transform.Translate(delta, Space.World);
        }

    }


    public override void StateExit()
    {
        base.StateExit();
        spriteRenderer.color = Color.white;
    }
}
