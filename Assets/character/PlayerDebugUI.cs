using UnityEngine;

public class PlayerDebugUI : MonoBehaviour
{
    public Player player; // 拖拽或自动获取

    [Header("显示设置")]
    public int barWidth = 200;
    public int barHeight = 20;
    public int marginLeft = 20;
    public int marginTop = 20;
    public int spacing = 10;

    private void Start()
    {
        if (player == null)
            player = GetComponent<Player>();
    }

    private void OnGUI()
    {
        if (player == null) return;

        float healthPercent = player.CurrentHealth / player.MaxHealth;

        // 血条
        Rect healthBarRect = new Rect(marginLeft, marginTop, barWidth, barHeight);
        DrawBar(healthBarRect, healthPercent, Color.red, "生命值");
        GUI.Label(new Rect(marginLeft + barWidth + 10, marginTop, 100, barHeight),
            $"{player.CurrentHealth:F0} / {player.MaxHealth:F0}");

        // 状态显示
        string status = "";
        if (player.IsDashing) status = "冲刺中";
        else if (player.IsRunning) status = "奔跑中";
        if (!string.IsNullOrEmpty(status))
            GUI.Label(new Rect(marginLeft, marginTop + barHeight + spacing, 150, 25), status);
    }

    private void DrawBar(Rect rect, float percent, Color color, string label)
    {
        GUI.Box(rect, "");
        Rect fillRect = new Rect(rect.x + 1, rect.y + 1, (rect.width - 2) * Mathf.Clamp01(percent), rect.height - 2);
        GUI.backgroundColor = color;
        GUI.Box(fillRect, "");
        GUI.backgroundColor = Color.white;
        GUI.Label(rect, label);
    }
}
