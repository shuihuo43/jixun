using System.Collections.Generic;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerState curState;
    private PlayerState initState;
    private Dictionary<string, PlayerState> stateLibrary;

    public string CurrentStateName => curState?.StateName;

    void Awake()
    {
        stateLibrary = new Dictionary<string, PlayerState>();
    }

    public void StateRegist(PlayerState state, string stateName, bool isInit = false)
    {
        if (stateLibrary == null)
            stateLibrary = new Dictionary<string, PlayerState>();

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

        curState.StateExit();
        curState = stateLibrary[tarName];
        curState.StateEnter();
    }

    public void StateUpdate()
    {
        if (curState != null)
            curState.StateUpdate();
    }

    public void StateFixedUpdate()
    {
        if (curState != null)
            curState.StateFixedUpdate();
    }
}
