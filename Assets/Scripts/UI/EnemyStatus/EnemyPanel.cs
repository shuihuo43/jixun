using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyPanel : MonoBehaviour
{
    [SerializeField] private Enemy enemy;
    [SerializeField] private Image healthProcessImg;

    [Header("Debuff")]
    [SerializeField] private DebuffListEntry[] debuffLists;

    [Serializable]
    public struct DebuffListEntry
    {
        public DebuffType type;
        public DebuffList list;
    }

    private EnemyResource resource;
    private Dictionary<DebuffType, DebuffList> listDict;

    void Start()
    {
        listDict = new();
        if (debuffLists != null)
            foreach (var e in debuffLists)
                listDict[e.type] = e.list;

        if (enemy == null)
        {
            Debug.LogWarning($"[EnemyPanel] enemy 未赋值，挂载在 {transform.root.name} 上");
            return;
        }
        if (enemy.resource == null)
        {
            Debug.LogWarning($"[EnemyPanel] enemy.resource 为 null，enemy={enemy.name}");
            return;
        }

        resource = enemy.resource;
        resource.OnHealthChanged += OnHealthChanged;
        resource.OnDeath += () => gameObject.SetActive(false);
        OnHealthChanged();
        Debug.Log($"[EnemyPanel] 初始化完成，enemy={enemy.name}, debuffLists={listDict.Count}");
    }

    void OnDestroy()
    {
        if (resource != null)
            resource.OnHealthChanged -= OnHealthChanged;
    }

    void OnHealthChanged()
    {
        if (healthProcessImg != null && resource.maxHealth > 0f)
            healthProcessImg.fillAmount = resource.currentHealth / resource.maxHealth;

        Debug.Log($"[EnemyPanel] OnHealthChanged, statusDict count={resource.statusDict.Count}, listDict count={listDict.Count}");
        foreach (var kv in listDict)
        {
            int count = resource.statusDict.TryGetValue(kv.Key, out int v) ? v : 0;
            Debug.Log($"[EnemyPanel] type={kv.Key}, count={count}, list={kv.Value?.name}");
            kv.Value?.SetCount(count, kv.Key);
        }
    }
}
