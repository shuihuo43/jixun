public class ActionNone : ActionState
{
    public ActionNone(string name, Player player, ActionStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }
}
