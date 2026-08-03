using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>可序列化的伤害类型→浮点字典，Inspector 可编辑</summary>
[Serializable]
public class DamageTypeFloatDict
{
    [SerializeField] private float physics = 1f;
    [SerializeField] private float ghost = 1f;
    [SerializeField] private float bleed = 1f;

    public float this[DamageType type]
    {
        get => type switch
        {
            DamageType.Physics => physics,
            DamageType.Ghost => ghost,
            DamageType.Bleed => bleed,
            _ => 1f,
        };
        set
        {
            switch (type)
            {
                case DamageType.Physics: physics = value; break;
                case DamageType.Ghost: ghost = value; break;
                case DamageType.Bleed: bleed = value; break;
            }
        }
    }
}

[CreateAssetMenu(fileName = "PlayerResource", menuName = "Resource/PlayerResource")]
public class PlayerResource : ScriptableObject
{
    public enum BoolBuffType
    {
        CanRecoverPendingBlood,    // 虚血恢复
        GhostSpawnWraithOnMark,    // 幽灵命中出血标记敌人生成恶灵
        GhostPersistOnHit,         // 幽灵穿透敌人
    }

    public enum ValueBuffType
    {
        MaxLife,              // 生命上限
        MaxEnergy,            // 精力上限
        EnergyRecoverySpeed,  // 精力恢复速度
        AttackInterval,       // 攻击间隔
        RunSpawnInterval,     // 奔跑生成间隔
        WeaponScale,          // 武器大小
        BleedDamage,          // 出血伤害
        BleedDuration,        // 出血持续时间
        BleedMaxStacks,       // 出血层数上限
        WraithMaxStacks,      // 冤魂层数上限
        GhostSpawnCount,      // 幽灵生成数量
        ComboThreshold,       // 连击触发阈值
        HealOnKill,            // 击杀回血
        HealOnHit,             // 命中回血
    }

    public enum OperatorType
    {
        Add,
        Sub,
        Mul,
        Div,
    }

    [Header("生命")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    public event Action OnHealthChanged;

    private Dictionary<ValueBuffType, Func<float>> valueGetters;
    private Dictionary<ValueBuffType, Action<float>> valueSetters;

    void OnEnable()
    {
        valueGetters = new()
        {
            [ValueBuffType.MaxLife] = () => maxHealth,
            [ValueBuffType.MaxEnergy] = () => maxEnergy,
            [ValueBuffType.EnergyRecoverySpeed] = () => recoverySpeed,
            [ValueBuffType.AttackInterval] = () => attackRoundInterval,
            [ValueBuffType.WeaponScale] = () => weaponScale,
            [ValueBuffType.RunSpawnInterval] = () => runSpawnInterval,
            [ValueBuffType.BleedDamage] = () => bleedDamage,
            [ValueBuffType.BleedDuration] = () => bleedDuration,
            [ValueBuffType.BleedMaxStacks] = () => bleedMaxStacks,
            [ValueBuffType.WraithMaxStacks] = () => wraithMaxStacks,
            [ValueBuffType.GhostSpawnCount] = () => ghostSpawnCount,
            [ValueBuffType.ComboThreshold] = () => comboThreshold,
            [ValueBuffType.HealOnKill] = () => healOnKill,
            [ValueBuffType.HealOnHit] = () => healOnHit,
        };
        valueSetters = new()
        {
            [ValueBuffType.MaxLife] = v => maxHealth = v,
            [ValueBuffType.MaxEnergy] = v => maxEnergy = v,
            [ValueBuffType.EnergyRecoverySpeed] = v => recoverySpeed = v,
            [ValueBuffType.AttackInterval] = v => attackRoundInterval = Mathf.Max(minAttackInterval, v),
            [ValueBuffType.WeaponScale] = v => weaponScale = v,
            [ValueBuffType.RunSpawnInterval] = v => runSpawnInterval = Mathf.Max(minRunSpawnInterval, v),
            [ValueBuffType.BleedDamage] = v => bleedDamage = v,
            [ValueBuffType.BleedDuration] = v => bleedDuration = Mathf.Max(0.1f, v),
            [ValueBuffType.BleedMaxStacks] = v => bleedMaxStacks = Mathf.RoundToInt(v),
            [ValueBuffType.WraithMaxStacks] = v => wraithMaxStacks = Mathf.RoundToInt(v),
            [ValueBuffType.GhostSpawnCount] = v => ghostSpawnCount = Mathf.RoundToInt(v),
            [ValueBuffType.ComboThreshold] = v => comboThreshold = Mathf.Max(1, Mathf.RoundToInt(v)),
            [ValueBuffType.HealOnKill] = v => healOnKill = Mathf.Max(0, v),
            [ValueBuffType.HealOnHit] = v => healOnHit = Mathf.Max(0, v),
        };
    }

    public void ChangeHealth(float delta)
    {
        currentHealth = Mathf.Clamp(currentHealth + delta, 0f, maxHealth);
        OnHealthChanged?.Invoke();
    }

    /// <summary>修改数值：传入要改的枚举、操作符、数值</summary>
    public void ModifyValue(ValueBuffType type, OperatorType op, float value)
    {
        if (!valueGetters.TryGetValue(type, out var get) || !valueSetters.TryGetValue(type, out var set))
            return;

        float cur = get();
        float result = op switch
        {
            OperatorType.Add => cur + value,
            OperatorType.Sub => cur - value,
            OperatorType.Mul => cur * value,
            OperatorType.Div => value != 0 ? cur / value : cur,
            _ => cur,
        };
        float delta = result - cur;
        set(result);

        if (type == ValueBuffType.MaxLife) currentHealth += delta;

        Debug.Log($"[PlayerResource] ModifyValue {type} {op} {value}: {cur} → {result}");
        OnHealthChanged?.Invoke();
    }

    [Header("能量")]
    public float maxEnergy = 100f;

    [Header("消耗")]
    public float dashCost = 20f;

    [Header("武器")]
    public WeaponResource[] WeaponResources;
    [Tooltip("冤魂/幽灵触碰敌人后是否不消失")]
    public bool wraithPersistOnHit = false;

    [Header("Bool Buff")]
    public HashSet<BoolBuffType> enabledBoolBuffs = new();

    public bool HasBoolBuff(BoolBuffType t) => enabledBoolBuffs.Contains(t);
    public void EnableBoolBuff(BoolBuffType t) => enabledBoolBuffs.Add(t);

    [Header("Modifier 列表")]
    public List<ModifierResource> modifiers = new();

    [Header("出血")]
    public float bleedDamage = 5f;
    public float bleedDuration = 1f;
    public int bleedMaxStacks = 10;

    [Header("幽灵")]
    public int wraithMaxStacks = 10;
    public int ghostSpawnCount = 1;
    public int comboThreshold = 10;
    public float healOnKill = 0f;
    public float healOnHit = 0f;

    [Header("连击生成")]
    public GameObject[] comboEntities; // 每 10 连击全生成一遍

    [Header("伤害类型增伤（默认 1=无增伤）")]
    public DamageTypeFloatDict damageBonuses = new();

    [Header("攻击轮次")]
    public float attackRoundInterval = 1f;
    public float minAttackInterval = 0.1f;
    public float weaponScale = 0.6f;

    [Header("奔跑生成")]
    public float runSpawnInterval = 1f;
    public ItemResource[] runSpawnItems;
    public float minRunSpawnInterval = 0.1f;

    [Header("恢复")]
    public float recoveryInterval = 1.5f;
    public float recoverySpeed = 0.15f;
}
