using System;
using System.Collections.Generic;
using UnityEngine;

public enum DebuffType
{
    WraithMark,
    Reap,
    Blood,
}

public enum DamageType
{
    Physics,   // 物理（玩家武器攻击）
    Ghost,     // 幽灵
    Bleed,     // 出血
}

[Serializable]
public struct DebuffEntry
{
    public DebuffType type;
    public int value;
}

[CreateAssetMenu(fileName = "DamageResource", menuName = "Resource/DamageResource")]
public class DamageResource : ScriptableObject
{
    public float baseDamageValue = 1f;
    public DamageType damageType = DamageType.Physics;
    public List<DebuffEntry> debuffList = new();

    public Dictionary<DebuffType, int> GetDebuffDict()
    {
        var dict = new Dictionary<DebuffType, int>();
        foreach (var e in debuffList)
            dict[e.type] = dict.TryGetValue(e.type, out int v) ? v + e.value : e.value;
        return dict;
    }
}
