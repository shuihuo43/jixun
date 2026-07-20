using System.Collections.Generic;

public class ActionStateMachine
{
    private ActionState curState;
    private ActionState initState;
    private Dictionary<string, ActionState> stateLibrary;

    public string CurrentStateName => curState?.StateName;

    public ActionStateMachine()
    {
        stateLibrary = new Dictionary<string, ActionState>();
    }

    public void StateRegist(ActionState state, string stateName, bool isInit = false)
    {
        if (stateLibrary == null)
            stateLibrary = new Dictionary<string, ActionState>();

        if (stateLibrary.ContainsKey(stateName))
            return;

        stateLibrary.Add(stateName, state);

        if (isInit)
        {
            if (initState != null) return;
            initState = state;
            curState = state;
            state.StateEnter();
        }
    }

    public void ChangeToState(string tarName)
    {
        if (!stateLibrary.ContainsKey(tarName))
            return;

        curState?.StateExit();
        curState = stateLibrary[tarName];
        curState.StateEnter();
    }

    public void StateUpdate()
    {
        curState?.StateUpdate();
    }

    public void StateFixedUpdate()
    {
        curState?.StateFixedUpdate();
    }
}
