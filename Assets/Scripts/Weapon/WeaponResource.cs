using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "WeaponResource", menuName = "Resource/WeaponResource")]
public class WeaponResource : ScriptableObject
{
    public enum ActionType
    {
        Swing = 0,
        Thrust = 1,
    }

    [SerializeField] public int AttackCount = 1;
    [SerializeField] public ActionType AttackType = ActionType.Swing;
    [SerializeField] public float AttackDuration = 0.3f;
    [SerializeField] public float AttackRecovery = 0.1f;
    [SerializeField] public float AttackAngleOffset = 0f;

    /// <summary>攻击总时长（含后摇）</summary>
    public float TotalDuration => AttackDuration + AttackRecovery;

}
