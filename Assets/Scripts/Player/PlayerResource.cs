using UnityEngine;

[CreateAssetMenu(fileName = "PlayerResource", menuName = "Resource/PlayerResource")]
public class PlayerResource : ScriptableObject
{
    [Header("能量")]
    public float maxEnergy = 100f;

    [Header("消耗")]
    public float dashCost = 20f;

    [Header("恢复")]
    public float recoveryInterval = 1.5f;   // 停止消耗后多久开始恢复
    public float recoverySpeed = 30f;       // 每秒恢复量
}
