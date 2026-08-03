using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ModifierContainer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Text countText;

    private ModifierResource resource;
    public ModifierResource Resource => resource;
    private int count;
    public event Action<ModifierResource, int> OnHover;
    public event Action OnHoverExit;

    void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnStateChanged;
        OnStateChanged(GameManager.Instance?.CurrentState ?? GameManager.GameState.Playing);
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    void OnStateChanged(GameManager.GameState state)
    {
        float a = state == GameManager.GameState.Paused ? 1f : 0.4f;
        if (iconImage != null) { var c = iconImage.color; c.a = a; iconImage.color = c; }
        if (countText != null) { var c = countText.color; c.a = a; countText.color = c; }
    }

    public void Refresh(ModifierResource res, int count)
    {
        resource = res;
        this.count = count;
        if (iconImage != null)
            iconImage.sprite = res?.sprite;
        if (countText != null)
            countText.text = count > 1 ? count.ToString() : "";
    }

    public void OnPointerEnter(PointerEventData e) => OnHover?.Invoke(resource, count);
    public void OnPointerExit(PointerEventData e) => OnHoverExit?.Invoke();
}
