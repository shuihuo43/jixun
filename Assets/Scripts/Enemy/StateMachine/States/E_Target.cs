using UnityEngine;

/// <summary>靶子状态：持续回血，不移动不切换</summary>
public class E_Target : EnemyState
{
    [SerializeField] private float healPerSecond = 10f;

    public override void StateEnter()
    {
        enemy.moveDir = Vector2.zero;
    }

    public override void StateUpdate()
    {
        if (enemy.resource != null)
            enemy.resource.ChangeHealth(healPerSecond * Time.deltaTime);
    }
}
