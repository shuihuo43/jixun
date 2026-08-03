using UnityEngine;

public class PlayerRun : PlayerState
{
    public PlayerRun(string name, Player player, PlayerStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }

    private float stopTimer;

    public override void StateEnter()
    {
        base.StateEnter();
        player.IsRunning = true;
        stopTimer = 0f;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        if (player.IsAttacking) return;

        if (!Input.GetKey(KeyCode.Space))
        {
            if (player.MoveInput != Vector2.zero)
                stateMachine.ChangeToState("Move");
            else
                stateMachine.ChangeToState("Idle");
            return;
        }

        if (player.MoveInput == Vector2.zero)
        {
            stopTimer += Time.deltaTime;
            if (stopTimer >= 0.3f)
                stateMachine.ChangeToState("Move");
        }
        else
        {
            stopTimer = 0f;
        }
    }

    public override void StateFixedUpdate()
    {
        base.StateFixedUpdate();
        player.ApplyMovement(player.RunSpeed);
        TrySpawn();
    }

    void TrySpawn()
    {
        var items = player.PlayerResource?.runSpawnItems;
        float interval = player.PlayerResource != null ? player.PlayerResource.runSpawnInterval : 1f;
        float slotTime = interval / 5f; // 固定 5 槽轮转

        // 初始化或当前槽时间到 → 切下一槽
        float elapsed = Time.time - player.itemTurnStart;
        if (player.itemTurnDuration <= 0f || elapsed >= slotTime)
        {
            player.itemSlotIdx = (player.itemSlotIdx + 1) % 5;
            player.itemTurnStart = Time.time;
            player.itemTurnDuration = slotTime;

            // 生成当前槽位物品，索敌最近敌人
            var item = items != null && player.itemSlotIdx < items.Length ? items[player.itemSlotIdx] : null;
            if (item?.prefab != null)
            {
                Vector2 dir = player.PreMovementNotZero;

                // 搜索最近敌人
                var enemies = Object.FindObjectsOfType<Enemy>();
                float nearest = float.MaxValue;
                Transform best = null;
                foreach (var e in enemies)
                {
                    if (e.resource != null && e.resource.currentHealth <= 0f) continue;
                    float d = Vector2.Distance(player.transform.position, e.transform.position);
                    if (d < nearest) { nearest = d; best = e.transform; }
                }
                if (best != null)
                    dir = ((Vector2)best.position - (Vector2)player.transform.position).normalized;

                var obj = Object.Instantiate(item.prefab, player.transform.position, Quaternion.identity);
                var entity = obj.GetComponent<Entity>();
                if (entity != null)
                    entity.EntityBorn(player.transform.position, dir, player.entityRoot, ownerObj: player.gameObject);
            }
        }
    }

    public override void StateExit()
    {
        base.StateExit();
        player.IsRunning = false;
    }
}
