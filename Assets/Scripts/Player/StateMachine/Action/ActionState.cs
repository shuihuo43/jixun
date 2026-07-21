public class ActionState
{
    public string StateName { get; private set; }
    protected Player player;
    protected ActionStateMachine stateMachine;

    public ActionState(string name, Player player, ActionStateMachine stateMachine, bool isInit = false)
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
