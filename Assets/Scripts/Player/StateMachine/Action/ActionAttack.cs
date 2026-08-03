using UnityEngine;

public class ActionAttack : ActionState
{
    private float slotTimer;
    private float slotCd;

    public ActionAttack(string name, Player player, ActionStateMachine stateMachine, bool isInit = false)
        : base(name, player, stateMachine, isInit) { }

    public override void StateEnter()
    {
        base.StateEnter();
        player.IsAttacking = true;
        player.IsSlowMove = true;
        slotTimer = 0f; // 立即推进到第一个有效武器
        slotCd = 0f;
        player.atkHitCount = 0;
    }

    public override void StateUpdate()
    {
        base.StateUpdate();

        slotTimer -= Time.deltaTime;

        // 槽时间到 → 推进到下一个有效武器
        if (slotTimer <= 0f)
        {
            AdvanceToNextValid();
            slotTimer += slotCd;
            player.atkHitCount = 0;
        }

        // UI 进度
        player.weaponTurnDuration = slotCd;
        player.weaponSlotTimer = slotTimer;

        // 松开 + 槽快结束时退出
        if (!Input.GetMouseButton(0) && slotTimer <= 0.02f)
        {
            stateMachine.ChangeToState("None");
            return;
        }

        TryFire(slotCd - slotTimer);
    }

    void AdvanceToNextValid()
    {
        var weapons = player.WeaponResources;
        if (weapons == null || weapons.Length == 0) return;

        // 有效武器数
        int validCount = 0;
        for (int i = 0; i < weapons.Length; i++)
            if (weapons[i] != null) validCount++;
        if (validCount == 0) return;

        slotCd = player.PlayerResource.attackRoundInterval / validCount;

        // 推进到下一个非空槽
        int start = player.atkWeaponIdx;
        do
        {
            player.atkWeaponIdx = (player.atkWeaponIdx + 1) % weapons.Length;
        }
        while (weapons[player.atkWeaponIdx] == null && player.atkWeaponIdx != start);
    }

    void TryFire(float slotElapsed)
    {
        var weapons = player.WeaponResources;
        if (weapons == null || weapons.Length == 0) return;

        int idx = player.atkWeaponIdx % weapons.Length;
        if (idx >= weapons.Length) return;
        var weapon = weapons[idx];
        if (weapon == null) return;
        if (slotCd <= 0f) return;

        int count = weapon.AttackCount;
        float hitCd = slotCd / count;

        int due = 1;
        if (count > 1 && slotElapsed > 0f)
            due += Mathf.FloorToInt((slotElapsed + 0.001f) / hitCd);
        if (due > count) due = count;

        while (player.atkHitCount < due)
        {
            player.AttackLogic();
            player.atkHitCount++;
        }
    }

    public override void StateExit()
    {
        base.StateExit();
        player.IsAttacking = false;
        player.IsSlowMove = false;
    }
}
