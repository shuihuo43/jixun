using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerResource", menuName = "Resource/PlayerResource")]
public class PlayerResource : ScriptableObject
{
    public enum BuffType
    {
        MaxHealth,
        MoveSpeed,
        RunSpeed,
        DashSpeed,
        DashCooldown,
        AttackDuration,
        AttackDamage,
        MaxEnergy,
        DashCost,
        EnergyRecoverySpeed,
    }

    [Header("生命")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    public event Action OnHealthChanged;
    public event Action OnBuffChanged;

    private Dictionary<BuffType, float> buffs = new();

    public void ChangeHealth(float delta)
    {
        currentHealth = Mathf.Clamp(currentHealth + delta, 0f, maxHealth);
        OnHealthChanged?.Invoke();
    }

    public void AddBuff(BuffType type, float value)
    {
        buffs.TryGetValue(type, out float cur);
        buffs[type] = cur + value;
        ApplyBuffToStat(type, value);
        OnBuffChanged?.Invoke();
    }

    public void RemoveBuff(BuffType type, float value)
    {
        buffs.TryGetValue(type, out float cur);
        buffs[type] = Mathf.Max(0, cur - value);
        ApplyBuffToStat(type, -value);
        OnBuffChanged?.Invoke();
    }

    public float GetBuff(BuffType type)
    {
        buffs.TryGetValue(type, out float v);
        return v;
    }

    void ApplyBuffToStat(BuffType type, float delta)
    {
        switch (type)
        {
            case BuffType.MaxHealth: maxHealth += delta; break;
            // case BuffType.MoveSpeed: ... 等 Player 那边读完 buff 自己算
        }
    }

    [Header("能量")]
    public float maxEnergy = 100f;

    [Header("消耗")]
    public float dashCost = 20f;

    [Header("恢复")]
    public float recoveryInterval = 1.5f;
    public float recoverySpeed = 30f;
}
