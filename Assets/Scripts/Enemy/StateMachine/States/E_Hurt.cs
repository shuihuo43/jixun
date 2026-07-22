using UnityEngine;

public class E_Hurt : EnemyState
{
    [SerializeField] private GameObject spriteObj;
    [SerializeField] private GameObject hurtEffectPrefab;
    [SerializeField] private int effectCount = 5;
    [SerializeField] private float knockbackDistance = 0.3f;
    [SerializeField] private float knockbackTime = 0.04f;
    [SerializeField] private float recoverTime = 0.08f;

    private Vector2 knockDir;
    private float timer;
    private bool recovering;

    public override void StateEnter()
    {
        timer = 0f;
        recovering = false;

        // 方向：远离玩家
        knockDir = enemy.Player != null
            ? ((Vector2)enemy.transform.position - (Vector2)enemy.Player.position).normalized
            : Vector2.right;

        // 重置位置
        if (spriteObj != null)
            spriteObj.transform.localPosition = Vector3.zero;

        SpawnHurtEffect();
    }

    public override void StateUpdate()
    {
        timer += Time.deltaTime;

        if (!recovering)
        {
            float t = Mathf.Clamp01(timer / knockbackTime);
            if (spriteObj != null)
                spriteObj.transform.localPosition = Vector2.Lerp(Vector2.zero, -knockDir * knockbackDistance, t);

            if (t >= 1f)
            {
                recovering = true;
                timer = 0f;
            }
        }
        else
        {
            float t = Mathf.Clamp01(timer / recoverTime);
            if (spriteObj != null)
                spriteObj.transform.localPosition = Vector2.Lerp(-knockDir * knockbackDistance, Vector2.zero, t);

            if (t >= 1f)
                stateMachine.ChangeToState("Engage");
        }
    }

    void SpawnHurtEffect()
    {
        if (hurtEffectPrefab == null || enemy.Player == null) return;

        float baseAngle = Mathf.Atan2(knockDir.y, knockDir.x) * Mathf.Rad2Deg;

        GameObject root = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
        if (root == null) root = enemy.gameObject;

        for (int i = 0; i < effectCount; i++)
        {
            float t = effectCount == 1 ? 0f : (float)i / (effectCount - 1);
            float angle = Mathf.Lerp(-90f, 90f, t) + baseAngle;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            var obj = Instantiate(hurtEffectPrefab, enemy.transform.position, Quaternion.identity);
            var entity = obj.GetComponent<Entity>();
            if (entity != null)
            {
                Vector2 localPos = root.transform.InverseTransformPoint(enemy.transform.position);
                entity.EntityBorn(localPos, dir, root);
            }
        }
    }
}
