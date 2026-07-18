using UnityEngine;

public class PlayerDebugUI : MonoBehaviour
{
    public PlayerController player;        // 拖拽或自动获取

    [Header("显示设置")]
    public int barWidth = 200;
    public int barHeight = 20;
    public int marginLeft = 20;
    public int marginTop = 20;
    public int spacing = 10;               // 两个条形之间的垂直间距

    private void Start()
    {
        if (player == null)
            player = GetComponent<PlayerController>();
    }

    private void OnGUI()
    {
        if (player == null) return;

        float healthPercent = player.CurrentHealth / player.MaxHealth;
        float staminaPercent = player.CurrentStamina / player.MaxStamina;

        // 绘制血条
        Rect healthBarRect = new Rect(marginLeft, marginTop, barWidth, barHeight);
        DrawBar(healthBarRect, healthPercent, Color.red, "生命值");
        // 绘制数值文本
        GUI.Label(new Rect(marginLeft + barWidth + 10, marginTop, 100, barHeight),
            $"{player.CurrentHealth:F0} / {player.MaxHealth:F0}");

        // 绘制精力条
        Rect staminaBarRect = new Rect(marginLeft, marginTop + barHeight + spacing, barWidth, barHeight);
        DrawBar(staminaBarRect, staminaPercent, Color.cyan, "精力值");
        GUI.Label(new Rect(marginLeft + barWidth + 10, marginTop + barHeight + spacing, 100, barHeight),
            $"{player.CurrentStamina:F0} / {player.MaxStamina:F0}");

        // 可选：显示冲刺/加速状态
        string status = "";
        if (player.IsDashing) status = "冲刺中";
        else if (player.IsAccelerating) status = "加速中";
        if (!string.IsNullOrEmpty(status))
            GUI.Label(new Rect(marginLeft, marginTop + 2 * (barHeight + spacing), 150, 25), status);
    }

    private void DrawBar(Rect rect, float percent, Color color, string label)
    {
        // 背景
        GUI.Box(rect, "");
        // 填充
        Rect fillRect = new Rect(rect.x + 1, rect.y + 1, (rect.width - 2) * Mathf.Clamp01(percent), rect.height - 2);
        GUI.backgroundColor = color;
        GUI.Box(fillRect, "");
        GUI.backgroundColor = Color.white;
        // 文字标签
        GUI.Label(rect, label);
    }
}