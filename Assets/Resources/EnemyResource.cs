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

    private bool dead;
    public Dictionary<DebuffType, int> statusDict = new();

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
            statusDict.TryGetValue(kv.Key, out int cur);
            statusDict[kv.Key] = cur + kv.Value;
        }
        OnHealthChanged?.Invoke();
    }
}
