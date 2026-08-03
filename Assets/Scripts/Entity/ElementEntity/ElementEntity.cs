using UnityEngine;

public enum ElementType
{
    None, Ghost, Wraith, LeafBlade,
}

public enum TrajectoryType
{
    None, Homing, Straight, Wander,
}

public class ElementEntity : Entity
{
    [Header("元素")]
    public ElementType elementType;
    public TrajectoryType trajectoryType;
    [TextArea] public string description;

    [Header("生命周期")]
    [SerializeField] protected float lifetime = 4f;
    protected float age;

    protected virtual void Update()
    {
        age += Time.deltaTime;
        if (age >= lifetime) EntityDestroy();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[{GetType().Name}] OnTriggerEnter2D other={other.name} tag={other.tag} age={age:F2}");
        if (!other.CompareTag("Enemy")) { Debug.Log($"  tag skip"); return; }
        if (age < 0.1f) { Debug.Log($"  age skip"); return; }
        bool persist = GameManager.Instance?.player?.PlayerResource?.HasBoolBuff(PlayerResource.BoolBuffType.GhostPersistOnHit) ?? false;
        Debug.Log($"  persist={persist}");
        if (!persist) EntityDestroy();
    }
}
