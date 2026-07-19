using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class PlayerState
{
    public PlayerStateMachine StateMechine { get; set; }
    private string stateName;
    private PlayerController player;
    private PlayerStateMachine stateMachine;

    public PlayerState(string name, PlayerController player, PlayerStateMachine stateMachine, bool isInit = false)
    {
        this.stateName = name;
        this.player = player;
        this.stateMachine = stateMachine;
        stateMachine.StateRegist(this, stateName, isInit);
    }

    public virtual void StateEnter()
    {
    }

    public virtual void StateUpdate()
    {
    }

    public virtual void StateFixedUpdate()
    {
    }

    public virtual void StateExit()
    {
    }

}
