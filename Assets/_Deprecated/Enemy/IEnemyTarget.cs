using UnityEngine;

/// <summary>索敌接口：EnemyBrain 通过此接口获取目标信息</summary>
public interface IEnemyTarget
{
    Transform Player { get; }
    bool IsPlayerDetected { get; }
    Vector2 DirectionToPlayer { get; }
    float DistanceToPlayer { get; }
    float DetectionRange { get; set; }
}
