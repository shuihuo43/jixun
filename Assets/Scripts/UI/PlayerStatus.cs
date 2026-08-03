using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [Header("血条")]
    [SerializeField] private Image healthBarBg;
    [SerializeField] private Image healthProcessImg;
    [SerializeField] private Image pendingBloodImg;
    [SerializeField] private float minWidth = 100f;
    [SerializeField] private float maxWidth = 450f;
    [SerializeField] private float baseHealth = 100f;
    [SerializeField] private float growthScale = 150f;

    [Header("连击")]
    [SerializeField] private GameObject numberPrefab;
    [SerializeField] private Transform comboRoot;

    private PlayerResource resource;
    private Player player;
    private float lastMaxHp;
    private int lastCombo = -1;
    private Color comboColor = Color.white;

    private static readonly Color[] ComboColors = {
        Color.white,
        new Color(1f, 0.3f, 0.3f),   // 红
        new Color(1f, 0.6f, 0.1f),   // 橙
        new Color(1f, 0.9f, 0.2f),   // 金
        new Color(0.3f, 1f, 0.3f),   // 绿
        new Color(0.3f, 0.6f, 1f),   // 蓝
        new Color(0.7f, 0.3f, 1f),   // 紫
    };

    void Start()
    {
        player = FindObjectOfType<Player>();
        if (player != null)
        {
            player.OnComboHit += OnComboHitEffect;
            player.OnComboTrigger += OnComboEffect;
            if (player.PlayerResource != null)
            {
                resource = player.PlayerResource;
                lastMaxHp = -1;
                resource.OnHealthChanged += Refresh;
                Refresh();
            }
        }
    }

    void OnDestroy()
    {
        if (player != null)
        {
            player.OnComboHit -= OnComboHitEffect;
            player.OnComboTrigger -= OnComboEffect;
        }
        if (resource != null) resource.OnHealthChanged -= Refresh;
    }

    void OnComboHitEffect()
    {
        var rt = comboRoot as RectTransform;
        if (rt == null) return;
        rt.localScale = new Vector3(1.08f, 1.08f, 1f);
    }

    void OnComboEffect()
    {
        StopAllCoroutines();
        StartCoroutine(ComboImpact());
    }

    System.Collections.IEnumerator ComboImpact()
    {
        var rt = comboRoot as RectTransform;
        if (rt == null) yield break;

        // 换色
        comboColor = ComboColors[player.comboStage % ComboColors.Length];

        // 冲击：放大 + 左倾
        rt.localScale = new Vector3(1.6f, 1.6f, 1f);
        rt.localRotation = Quaternion.Euler(0, 0, 12f);

        float t2 = 0f;
        while (t2 < 0.2f)
        {
            t2 += Time.unscaledDeltaTime;
            float p = t2 / 0.2f;
            rt.localScale = Vector3.Lerp(new Vector3(1.6f, 1.6f, 1f), Vector3.one, p * p);
            rt.localRotation = Quaternion.Euler(0, 0, Mathf.Lerp(12f, 0f, p * p));
            yield return null;
        }

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
    }

    void Update()
    {
        if (player == null) return;

        // 每帧同步两个 fillAmount
        if (resource != null)
        {
            healthProcessImg.fillAmount = resource.currentHealth / resource.maxHealth;
            if (pendingBloodImg != null)
            {
                bool canRecover = player.PlayerResource.HasBoolBuff(PlayerResource.BoolBuffType.CanRecoverPendingBlood);
                pendingBloodImg.gameObject.SetActive(canRecover);
                pendingBloodImg.fillAmount = player.pendingBloodMax / resource.maxHealth;
            }
        }

        float fill = player.comboMaxTime > 0f ? player.comboTime / player.comboMaxTime : 0f;

        if (player.combo != lastCombo)
        {
            lastCombo = player.combo;
            UpdateCombo();
        }

        // 颜色 + 透明度
        foreach (Transform t in comboRoot)
        {
            var img = t.GetComponent<Image>();
            if (img != null) { comboColor.a = fill; img.color = comboColor; }
        }

        if (player.combo == 0) comboColor = Color.white;
        comboRoot.gameObject.SetActive(player.combo > 0);
    }

    void UpdateCombo()
    {
        foreach (Transform t in comboRoot) Destroy(t.gameObject);

        int val = player.combo;
        if (val == 0) return;

        while (val > 0)
        {
            var obj = Instantiate(numberPrefab, comboRoot);
            obj.transform.SetAsLastSibling();
            obj.GetComponent<ComboNumber>()?.SetDigit(val % 10);
            val /= 10;
        }
    }

    void Refresh()
    {
        if (healthProcessImg == null || resource == null) return;

        // 宽度：基于 100 的指数增长趋近 450
        if (resource.maxHealth != lastMaxHp)
        {
            lastMaxHp = resource.maxHealth;
            float delta = resource.maxHealth - baseHealth;
            float t = 1f - Mathf.Exp(-Mathf.Max(0f, delta) / growthScale);
            float w = Mathf.Min(maxWidth, minWidth + (maxWidth - minWidth) * t);

            // 固定左边，往右延长
            var prt = healthProcessImg.rectTransform;
            prt.anchorMin = new Vector2(0f, prt.anchorMin.y);
            prt.anchorMax = new Vector2(0f, prt.anchorMax.y);
            prt.pivot = new Vector2(0f, prt.pivot.y);
            prt.sizeDelta = new Vector2(w, prt.sizeDelta.y);

            if (healthBarBg != null)
            {
                var brt = healthBarBg.rectTransform;
                brt.anchorMin = new Vector2(0f, brt.anchorMin.y);
                brt.anchorMax = new Vector2(0f, brt.anchorMax.y);
                brt.pivot = new Vector2(0f, brt.pivot.y);
                brt.sizeDelta = new Vector2(w, brt.sizeDelta.y);
            }

        }

        healthProcessImg.fillAmount = resource.currentHealth / resource.maxHealth;

        if (pendingBloodImg != null)
        {
            // 位置尺寸完全复制血条
            pendingBloodImg.rectTransform.anchoredPosition = healthProcessImg.rectTransform.anchoredPosition;
            pendingBloodImg.rectTransform.sizeDelta = healthProcessImg.rectTransform.sizeDelta;
            pendingBloodImg.rectTransform.anchorMin = healthProcessImg.rectTransform.anchorMin;
            pendingBloodImg.rectTransform.anchorMax = healthProcessImg.rectTransform.anchorMax;
            pendingBloodImg.rectTransform.pivot = healthProcessImg.rectTransform.pivot;
            if (player != null)
                pendingBloodImg.fillAmount = player.pendingBloodMax / resource.maxHealth;
        }
    }
}
