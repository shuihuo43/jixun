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

    private PlayerStatePanel panel;

    public virtual void StateEnter()
    {
        if (panel == null)
            panel = Object.FindObjectOfType<PlayerStatePanel>();

        if (panel != null && player.PlayerResource != null)
        {
            panel.SetDashCost(player.PlayerResource.dashCost, player.PlayerResource.maxEnergy);
            panel.UpdateEnergy(player.CurrentEnergy, player.MaxEnergy);
        }

        player.OnEnergyChanged += UpdateEnergy;
    }

    public virtual void StateUpdate() { }
    public virtual void StateFixedUpdate() { }

    public virtual void StateExit()
    {
        player.OnEnergyChanged -= UpdateEnergy;
    }

    /// <summary>精力变化回调，自动同步 UI 面板</summary>
    protected virtual void UpdateEnergy()
    {
        if (panel != null && player.PlayerResource != null)
            panel.UpdateEnergy(player.CurrentEnergy, player.MaxEnergy);
    }
}
