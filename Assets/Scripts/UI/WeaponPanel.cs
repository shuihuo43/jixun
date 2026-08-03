using UnityEngine;
using UnityEngine.UI;

public class WeaponPanel : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text damageText;
    [SerializeField] private Text attackCountText;
    [SerializeField] private Text descText;
    [SerializeField] private Text debuffText;
    [SerializeField] private CanvasGroup canvasGroup;

    void Start()
    {
        // 阻止面板内部元素拦截鼠标事件
        foreach (var g in GetComponentsInChildren<Graphic>())
            g.raycastTarget = false;
        Hide();
    }

    public void Show(WeaponResource weapon)
    {
        if (weapon == null) { Hide(); return; }
        if (canvasGroup != null) { canvasGroup.alpha = 1f; canvasGroup.blocksRaycasts = false; canvasGroup.interactable = false; }
        else gameObject.SetActive(true);

        if (iconImage != null) iconImage.sprite = weapon.icon;
        if (nameText != null) nameText.text = weapon.weaponName;
        if (damageText != null) damageText.text = $"伤害：{weapon.damageResource?.baseDamageValue ?? 0}";
        if (attackCountText != null) attackCountText.text = $"攻击次数：{weapon.AttackCount}";
        if (descText != null) descText.text = weapon.description;
        if (debuffText != null) debuffText.text = GetDebuffText(weapon.damageResource);
    }

    string GetDebuffText(DamageResource dmg)
    {
        if (dmg?.debuffList == null || dmg.debuffList.Count == 0) return "异常：无";

        var parts = new System.Collections.Generic.List<string>();
        foreach (var e in dmg.debuffList)
        {
            string name = e.type switch { 
                DebuffType.WraithMark => "冤魂", 
                DebuffType.Reap => "收割",
                DebuffType.Blood => "出血",
                _ => "???" 
            };
            parts.Add($"{name}+{e.value}");
        }
        return $"异常：{string.Join(" ", parts)}";
    }

    public void Hide()
    {
        if (canvasGroup != null) { canvasGroup.alpha = 0f; canvasGroup.blocksRaycasts = false; }
        else gameObject.SetActive(false);
    }
}
