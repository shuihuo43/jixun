using UnityEngine;

public class E_02_Attack : E_Attack
{
    [SerializeField] private int bulletCount = 3;
    [SerializeField] private float bulletSpreadAngle = 30f;
    [SerializeField] private float predictionTime = 0.3f;
    [SerializeField] private bool dirToTarget = true;
    [SerializeField] private Vector2 shotDir = Vector2.right;

    public override void StateEnter()
    {
        base.StateEnter();
        enemy.moveDir = enemy.faceDir;
    }

    public override void StateFixedUpdate()
    {
        Vector2 delta = enemy.moveDir * enemy.moveSpeed * Time.fixedDeltaTime;
        if (enemy.Rigidbody2D != null)
            enemy.Rigidbody2D.MovePosition(enemy.Rigidbody2D.position + delta);
    }

    protected override void OnAttack()
    {
        enemy.StartAttackCooldown();

        if (attackPrefab == null)
        {
            Debug.LogWarning("E_02_Attack: attackPrefab is null");
            OnAttackEnd();
            return;
        }

        GameObject root = GameObject.FindGameObjectWithTag("EnemyAttackEntityRoot");
        if (root == null) root = enemy.gameObject;

        // 瞄准方向
        Vector2 aimDir;
        if (dirToTarget)
        {
            Vector3 predictedPos = enemy.Player.position;
            Player player = enemy.Player.GetComponent<Player>();
            if (player != null && player.MoveInput.sqrMagnitude > 0.01f)
                predictedPos += (Vector3)(player.MoveInput * player.MoveSpeed * predictionTime);

            aimDir = (predictedPos - enemy.transform.position).normalized;
        }
        else
        {
            aimDir = shotDir.normalized;
        }

        float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        float halfSpan = bulletSpreadAngle * (bulletCount - 1) / 2f;

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = -halfSpan + bulletSpreadAngle * i + baseAngle;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

            GameObject obj = Instantiate(attackPrefab, enemy.transform.position, Quaternion.identity);
            BulletEntity bullet = obj.GetComponent<BulletEntity>();
            if (bullet != null)
            {
                Vector2 localPos = root.transform.InverseTransformPoint(enemy.transform.position);
                bullet.EntityBorn(localPos, dir, root, ownerObj: enemy.gameObject);
            }
        }

        OnAttackEnd();
    }
}
