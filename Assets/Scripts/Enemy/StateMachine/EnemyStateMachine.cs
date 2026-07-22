using System.Collections.Generic;
using UnityEngine;

/// <summary>通用敌人状态机——拖拽状态脚本到数组，自动注册到字典</summary>
public class EnemyStateMachine : MonoBehaviour
{
    [SerializeField] private EnemyState[] stateList;
    [SerializeField] private EnemyState initState;

    private readonly Dictionary<string, EnemyState> states = new();
    private EnemyState currentState;
    private Enemy enemy;
    public string CurrentStateName => currentState?.StateName;

    void Awake()
    {
        enemy = GetComponent<Enemy>();

        foreach (var s in stateList)
        {
            if (s == null) continue;
            s.Init(enemy, this);
            states[s.StateName] = s;
        }

        if (initState != null)
            ChangeToState(initState.StateName);
    }

    void Update()
    {
        currentState?.StateUpdate();
    }

    void FixedUpdate()
    {
        currentState?.StateFixedUpdate();
    }

    public void ChangeToState(string name)
    {
        if (!states.TryGetValue(name, out var next))
        {
            Debug.LogError($"EnemyStateMachine: 状态 '{name}' 未注册");
            return;
        }

        currentState?.StateExit();
        currentState = next;
        currentState.StateEnter();
    }
}
