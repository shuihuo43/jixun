using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyResource", menuName = "Resource/EnemyResource")]
public class EnemyResource : ScriptableObject
{
    public float maxHealth = 50f;
    public float currentHealth = 50f;

    public event Action OnHealthChanged;
    public event Action OnDeath;
    public event Action OnReap;

    private bool dead;
    public Dictionary<DebuffType, int> statusDict = new();

    /// <summary>运行时由 Enemy 从 PlayerResource 同步</summary>
    [System.NonSerialized] public int wraithMaxStacks = 10;
    [System.NonSerialized] public int bleedMaxStacks = 10;
    [System.NonSerialized] public float bleedDuration = 1f;

    private float bleedTickTimer; // 统一倒计时，归零减一层

    public void ChangeHealth(float delta)
    {
        if (dead) return;

        currentHealth = Mathf.Clamp(currentHealth + delta, 0f, maxHealth);
        OnHealthChanged?.Invoke();

        if (currentHealth <= 0f)
        {
            dead = true;
            OnDeath?.Invoke();
        }
    }

    public void ApplyDebuffs(Dictionary<DebuffType, int> debuffs)
    {
        if (debuffs == null) return;
        foreach (var kv in debuffs)
        {
            if (kv.Key == DebuffType.Reap)
            {
                for (int i = 0; i < kv.Value; i++)
                    OnReap?.Invoke();
                statusDict[DebuffType.WraithMark] = 0;
                continue;
            }

            statusDict.TryGetValue(kv.Key, out int cur);
            var val = cur + kv.Value;
            if (kv.Key == DebuffType.WraithMark && val > wraithMaxStacks) val = wraithMaxStacks;
            if (kv.Key == DebuffType.Blood)
            {
                if (val > bleedMaxStacks) val = bleedMaxStacks;
                // 从 0 到有层数时初始化计时器
                if (cur == 0 && val > 0)
                    bleedTickTimer = bleedDuration;
            }
            statusDict[kv.Key] = val;
        }
        OnHealthChanged?.Invoke();
    }

    /// <summary>统一计时，每次归零减一层出血</summary>
    public void UpdateBleedExpiry(float dt)
    {
        if (!statusDict.TryGetValue(DebuffType.Blood, out int stacks) || stacks <= 0) return;

        bleedTickTimer -= dt;
        if (bleedTickTimer <= 0f)
        {
            bleedTickTimer += bleedDuration;
            statusDict[DebuffType.Blood] = stacks - 1;
        }
    }
}
