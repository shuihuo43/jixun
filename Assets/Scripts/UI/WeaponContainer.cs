using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class WeaponContainer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite defaultIcon;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image processImage; // Filled 类型，显示轮转进度

    // 颜色常量
    private static readonly Color PlayingBg = new Color(1f, 1f, 1f, 0.25f);        // 白色 1/4
    private static readonly Color PausedBg = new Color(0.6f, 0.8f, 1f, 0.25f);     // 浅蓝 1/4
    private static readonly Color SelectedBg = new Color(0.6f, 0.8f, 1f, 1f);      // 浅蓝实心

    private WeaponResource resource;
    public WeaponResource Resource => resource;
    private WeaponList list;
    private bool isSelected;
    private bool isHovered;
    public event Action OnClickEvent;

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnStateChanged;
        UpdateBg();
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    void OnStateChanged(GameManager.GameState state)
    {
        UpdateBg();
        bool paused = state == GameManager.GameState.Paused;
        if (processImage != null) processImage.gameObject.SetActive(!paused);
    }

    /// <summary>设置轮转进度 fillAmount（0~1）</summary>
    public void SetProcessFill(float fill)
    {
        if (processImage != null)
            processImage.fillAmount = fill;
    }

    public void Refresh(WeaponResource weapon, WeaponList list)
    {
        this.list = list;
        resource = weapon;
        if (iconImage != null)
        {
            iconImage.sprite = weapon?.icon ?? defaultIcon;
            iconImage.raycastTarget = true; // 空槽也要能点击
        }
        UpdateBg();
    }

    public void SetHighlight(bool on)
    {
        isSelected = on;
        UpdateBg();
    }

    public void OnPointerEnter(PointerEventData e)
    {
        isHovered = true;
        UpdateBg();

        // 暂停状态 + 有武器 → 显示详情面板
        bool paused = GameManager.Instance?.CurrentState == GameManager.GameState.Paused;
        if (paused && resource != null)
            list?.OnHoverWeapon(resource);
    }

    public void OnPointerExit(PointerEventData e)
    {
        isHovered = false;
        UpdateBg();

        bool paused = GameManager.Instance?.CurrentState == GameManager.GameState.Paused;
        if (paused)
            list?.OnHoverExit();
    }

    public void OnClick() => OnClickEvent?.Invoke();

    void UpdateBg()
    {
        if (bgImage == null) return;

        if (isSelected)
        {
            bgImage.color = SelectedBg;
        }
        else if (isHovered)
        {
            bool paused = GameManager.Instance?.CurrentState == GameManager.GameState.Paused;
            Color c = paused ? PausedBg : PlayingBg;
            c.a = 0.5f;
            bgImage.color = c;
        }
        else
        {
            bool paused = GameManager.Instance?.CurrentState == GameManager.GameState.Paused;
            bgImage.color = paused ? PausedBg : PlayingBg;
        }
    }
}
