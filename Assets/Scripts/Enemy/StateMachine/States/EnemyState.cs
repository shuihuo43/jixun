using UnityEngine;

/// <summary>敌人状态基类——只持有敌人引用，由 EnemyStateMachine.Init() 初始化</summary>
public abstract class EnemyState : MonoBehaviour
{
    [SerializeField] protected string stateName;
    public string StateName => stateName;

    protected Enemy enemy;
    protected EnemyStateMachine stateMachine;

    /// <summary>由 EnemyStateMachine.Awake 调用，设置引用</summary>
    public void Init(Enemy enemy, EnemyStateMachine sm)
    {
        this.enemy = enemy;
        this.stateMachine = sm;
        OnInit();
    }

    protected virtual void OnInit() { }
    public virtual void StateEnter() { }
    public virtual void StateUpdate() { }
    public virtual void StateFixedUpdate() { }
    public virtual void StateExit() { }
}
