using UnityEngine;

/// <summary>定向移动：沿 enemy.moveDir 以 enemy.moveSpeed 移动</summary>
public class E_Move : EnemyState
{
    public override void StateEnter()
    {
        if (enemy.moveDir == Vector2.zero)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            enemy.moveDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }

    public override void StateFixedUpdate()
    {
        Vector2 delta = enemy.moveDir * enemy.moveSpeed * Time.fixedDeltaTime;
        if (enemy.Rigidbody2D != null)
            enemy.Rigidbody2D.MovePosition(enemy.Rigidbody2D.position + delta);
        else
            enemy.transform.Translate(delta, Space.World);
    }
}
