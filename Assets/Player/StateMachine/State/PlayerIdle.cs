using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerIdle : PlayerState
{
    public PlayerIdle(string name, PlayerController player, PlayerStateMachine stateMachine, bool isInit = false) : base(name, player, stateMachine, isInit)
    {
    }

    public override void StateEnter()
    {
        base.StateEnter();

    }

    public override void StateExit()
    {
        base.StateExit();
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();
    }

    public override void StateUpdate()
    {
        base.StateUpdate();
    }
}
