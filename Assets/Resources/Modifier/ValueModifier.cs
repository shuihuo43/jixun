using UnityEngine;

[CreateAssetMenu(fileName = "ValueModifier", menuName = "Resource/ValueModifier")]
public class ValueModifier : ModifierResource
{
    public PlayerResource.ValueBuffType buffType;
    public PlayerResource.OperatorType opType;
    public float value;

    public override void GetTreasure()
    {
        var player = GameManager.Instance?.player;
        Debug.Log($"[ValueModifier] GetTreasure: player={player?.name}, buffType={buffType}, op={opType}, val={value}");
        if (player != null && player.PlayerResource != null)
        {
            player.PlayerResource.ModifyValue(buffType, opType, value);
            player.PlayerResource.modifiers.Add(this);
        }
        else
            Debug.LogWarning("[ValueModifier] player or PlayerResource is null");
    }

    public override string GetDescription()
    {
        return description.Replace("[value]", $"[{value}]");
    }
}
