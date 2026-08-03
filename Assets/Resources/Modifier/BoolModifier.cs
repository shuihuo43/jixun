using UnityEngine;

[CreateAssetMenu(fileName = "BoolModifier", menuName = "Resource/BoolModifier")]
public class BoolModifier : ModifierResource
{
    public PlayerResource.BoolBuffType buffType;

    public override void GetTreasure()
    {
        var player = GameManager.Instance?.player;
        player?.PlayerResource?.EnableBoolBuff(buffType);
        if (player?.PlayerResource != null)
            player.PlayerResource.modifiers.Add(this);
    }
}
