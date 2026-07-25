using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyResource", menuName = "Resource/EnemyResource")]
public class EnemyResource : ScriptableObject
{
    public float maxHealth = 50f;
    public float currentHealth = 50f;

    public event Action OnHealthChanged;
    public event Action OnDeath;

    private bool dead;

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
}
