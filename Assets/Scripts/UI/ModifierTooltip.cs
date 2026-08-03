using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class ModifierTooltip : MonoBehaviour
{
    [SerializeField] private Text descText;
    [SerializeField] private Vector2 offset = new Vector2(20f, 0f);
    [SerializeField] private Vector2 padding = new Vector2(10f, 10f);

    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (descText != null) descText.supportRichText = true;
        var rt = (RectTransform)transform;
        rt.pivot = new Vector2(0f, 1f);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.localScale = Vector3.one;
        Hide();
    }

    void Update()
    {
        Vector2 pos = Input.mousePosition;
        pos += offset;
        transform.position = pos;
    }

    public void Show(ModifierResource res, int count)
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Paused) return;
        if (descText != null && res != null)
        {
            string desc = res.GetDescription();
            desc = System.Text.RegularExpressions.Regex.Replace(desc, @"\[(\d+)\]", m =>
            {
                int val = int.Parse(m.Groups[1].Value);
                int result = val * count;
                return $"{result} <color=#aaaaaa>// {val} * {count}</color>";
            });
            descText.text = desc;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
            float w = descText.preferredWidth + padding.x * 2f;
            float h = descText.fontSize * 2f;
            ((RectTransform)transform).sizeDelta = new Vector2(w, h);
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;
    }

    public void Hide()
    {
        if (canvasGroup != null) canvasGroup.alpha = 0f;
    }
}
