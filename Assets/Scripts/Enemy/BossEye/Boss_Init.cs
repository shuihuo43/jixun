using UnityEngine;

public class Boss_Init : EnemyState
{
    [SerializeField] private BossInitTrigger triggerObj;

    bool triggered;

    public override void StateEnter()
    {
        enemy.moveDir = Vector2.zero;
        triggered = false;
        if (triggerObj != null) triggerObj.onTriggered += OnPlayerDetected;
        Debug.Log($"[Boss_Init] entered, triggerObj={triggerObj?.name}");
    }

    public override void StateExit()
    {
        if (triggerObj != null) triggerObj.onTriggered -= OnPlayerDetected;
    }

    void OnPlayerDetected()
    {
        if (triggered) return;
        triggered = true;
        Debug.Log("[Boss_Init] Player detected, switching to Follow");
        stateMachine.ChangeToState("Follow");
    }
}
