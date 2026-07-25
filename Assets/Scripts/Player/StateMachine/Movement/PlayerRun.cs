using UnityEngine;

public class PlayerRun : PlayerState
{
    public PlayerRun(string name, Player player, PlayerStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }

    public override void StateEnter()
    {
        base.StateEnter();
        player.IsRunning = true;
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
        var prefs = player.runSpawnPrefabs;
        if (prefs == null || prefs.Length == 0) return;

        float perDelay = player.runSpawnInterval / prefs.Length;
        player.runSpawnTimer -= Time.fixedDeltaTime;
        if (player.runSpawnTimer <= 0f)
        {
            player.runSpawnTimer = player.runSpawnInterval;
            player.runSpawnIndex = -1;
        }

        float elapsed = player.runSpawnInterval - player.runSpawnTimer;
        int idx = Mathf.FloorToInt(elapsed / perDelay);
        if (idx > player.runSpawnIndex)
        {
            player.runSpawnIndex = idx;
            var prefab = prefs[idx % prefs.Length];
            if (prefab != null)
            {
                var obj = Object.Instantiate(prefab, player.transform.position, Quaternion.identity);
                var entity = obj.GetComponent<Entity>();
                if (entity != null)
                    entity.EntityBorn(player.transform.position, player.PreMovementNotZero, player.entityRoot, ownerObj: player.gameObject);
            }
        }
    }

    public override void StateExit()
    {
        base.StateExit();
        player.IsRunning = false;
    }
}
