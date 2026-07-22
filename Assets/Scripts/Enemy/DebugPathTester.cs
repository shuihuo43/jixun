using UnityEngine;

/// <summary>调试用：拖到敌人上，运行时每帧算路径显示 Gizmo</summary>
public class DebugPathTester : MonoBehaviour
{
    public EnemyPathfinding pathfinding;
    public Transform target;

    void Update()
    {
        if (pathfinding == null || target == null) return;
        pathfinding.debugPath = pathfinding.FindPath(transform.position, target.position);
    }
}
