using UnityEngine;
using UnityEngine.UI;

public class PlayerStatePanel : MonoBehaviour
{
    [Header("条")]
    [SerializeField] private Image healthProcessImg;
    [SerializeField] private Image energyProcessImg;

    [Header("Dash 消耗指示")]
    [SerializeField] private Image dashCast;

    [Header("Dash 颜色")]
    [SerializeField] private Color dashCastFullColor = Color.white;
    [SerializeField] private Color dashCastLowColor = Color.red;

    private float dashCost;
    private float maxEnergy;

    public void SetDashCost(float dashCost, float maxEnergy)
    {
        this.dashCost = dashCost;
        this.maxEnergy = maxEnergy;
    }

    public void UpdateHealth(float health, float maxHealth)
    {
        healthProcessImg.fillAmount = health / maxHealth;
    }

    public void UpdateEnergy(float energy, float maxEnergy)
    {
        energyProcessImg.fillAmount = energy / maxEnergy;
        AdjustDashCast();
    }

    private void AdjustDashCast()
    {
        if (maxEnergy <= 0f) return;

        float w = energyProcessImg.rectTransform.rect.width;
        float energyPct = energyProcessImg.fillAmount;
        float dashPct = dashCost / maxEnergy;

        float x = energyPct * w - w * dashPct;
        float fill = dashPct;

        // 左边界保护：超出时 x 钉在左边，fill 缩到当前精力
        if (x < 0f)
        {
            x = 0f;
            fill = energyPct;
        }

        dashCast.rectTransform.anchoredPosition = new Vector2(x, dashCast.rectTransform.anchoredPosition.y);
        dashCast.fillAmount = fill;
        dashCast.color = Color.Lerp(dashCastLowColor, dashCastFullColor, energyPct);
    }
}
