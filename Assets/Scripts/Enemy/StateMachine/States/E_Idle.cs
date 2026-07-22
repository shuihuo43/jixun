using UnityEngine;

/// <summary>纯待机：不动，等一会进入游走</summary>
public class E_Idle : EnemyState
{
    [SerializeField] private float idleDuration = 1.5f;
    private float timer;

    public override void StateEnter()
    {
        enemy.moveDir = Vector2.zero;
        timer = idleDuration;
    }

    public override void StateUpdate()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            stateMachine.ChangeToState("Wander");
    }
}
