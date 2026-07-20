using UnityEngine;

public class PlayerState
{
    public string StateName { get; private set; }
    protected Player player;
    protected PlayerStateMachine stateMachine;

    public PlayerState(string name, Player player, PlayerStateMachine stateMachine, bool isInit = false)
    {
        this.StateName = name;
        this.player = player;
        this.stateMachine = stateMachine;
        stateMachine.StateRegist(this, name, isInit);
    }

    public virtual void StateEnter() { }
    public virtual void StateUpdate() { }
    public virtual void StateFixedUpdate() { }
    public virtual void StateExit() { }
}
