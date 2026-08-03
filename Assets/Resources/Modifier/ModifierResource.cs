using UnityEngine;

[CreateAssetMenu(fileName = "ModifierResource", menuName = "Resource/ModifierResource")]
public class ModifierResource : ScriptableObject
{
    public Sprite sprite;
    [TextArea] public string description;

    public virtual void GetTreasure() { }

    public virtual string GetDescription() => description;
}
