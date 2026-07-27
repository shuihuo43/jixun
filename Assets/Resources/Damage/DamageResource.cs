using System;
using System.Collections.Generic;
using UnityEngine;

public enum DebuffType
{
    WraithMark,
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
    public List<DebuffEntry> debuffList = new();

    public Dictionary<DebuffType, int> GetDebuffDict()
    {
        var dict = new Dictionary<DebuffType, int>();
        foreach (var e in debuffList)
            dict[e.type] = dict.TryGetValue(e.type, out int v) ? v + e.value : e.value;
        return dict;
    }
}
