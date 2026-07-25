using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    [Header("血条")]
    [SerializeField] private Image healthBarBg;
    [SerializeField] private Image healthProcessImg;
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

    void Start()
    {
        player = FindObjectOfType<Player>();
        if (player != null && player.PlayerResource != null)
        {
            resource = player.PlayerResource;
            lastMaxHp = -1;
            resource.OnHealthChanged += Refresh;
            Refresh();
        }
    }

    void Update()
    {
        if (player == null) return;

        float fill = player.comboMaxTime > 0f ? player.comboTime / player.comboMaxTime : 0f;

        if (player.combo != lastCombo)
        {
            lastCombo = player.combo;
            UpdateCombo();
        }

        // 动态透明度
        foreach (Transform t in comboRoot)
        {
            var img = t.GetComponent<Image>();
            if (img != null) { var c = img.color; c.a = fill; img.color = c; }
        }

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

    void OnDestroy()
    {
        if (resource != null) resource.OnHealthChanged -= Refresh;
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
    }
}
