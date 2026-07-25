using UnityEngine;

public class EnemyAttackEntity : Entity
{
    public enum AttackType 
    {
        Poison_01 = 0,
    }

    [Header("组件")]
    [SerializeField] private Animator animator;

    [Header("检测")]
    [SerializeField] private ShapeArea shapeArea;
    [SerializeField] private string targetTag = "Player";

    [Header("攻击参数")]
    [SerializeField] private AttackType attackType;

    private bool detecting;

    public override void EntityBorn(Vector2 position, Vector2 direction, GameObject bornRoot, Vector2? scale = null, bool flipY = false, System.Action onDestroy = null, GameObject ownerObj = null)
    {
        base.EntityBorn(position, direction, bornRoot, scale, flipY, onDestroy, ownerObj);

        if (animator != null)
        {
            animator.SetInteger("AttackType", (int)attackType);
            animator.SetTrigger("Attack");
        }
    }

    /// <summary>动画帧调用：开始检测</summary>
    public void StartDetection()
    {
        detecting = true;
    }

    /// <summary>动画帧调用：停止检测</summary>
    public void StopDetection()
    {
        detecting = false;
    }

    void Update()
    {
        if (!detecting || shapeArea == null) return;

        var hit = shapeArea.DetectTag(targetTag);
        if (hit != null)
        {
            var player = hit.GetComponent<Player>();
            if (player != null)
                player.Hurt(this);
            StopDetection();
        }
    }
}
