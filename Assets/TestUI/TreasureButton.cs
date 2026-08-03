using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TreasureButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    public ModifierResource resource;
    public event Action<ModifierResource> OnHover;
    public event Action OnHoverExit;

    public void Init(ModifierResource res)
    {
        resource = res;
        if (iconImage != null && res?.sprite != null)
            iconImage.sprite = res.sprite;

        var btn = GetComponent<Button>();
        if (btn != null)
            btn.onClick.AddListener(() => resource?.GetTreasure());
    }

    public void OnPointerEnter(PointerEventData e) => OnHover?.Invoke(resource);
    public void OnPointerExit(PointerEventData e) => OnHoverExit?.Invoke();
}
