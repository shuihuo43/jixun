using UnityEngine;

public class PlayerBag : MonoBehaviour
{
    [SerializeField] private WeaponList weaponList;
    [SerializeField] private ItemList itemList;
    [SerializeField] private WeaponPanel hoverPanel;
    [SerializeField] private WeaponPanel selectedPanel;

    void Start()
    {
        if (weaponList != null) weaponList.bag = this;
        if (itemList != null) itemList.bag = this;
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnStateChanged -= OnStateChanged;
    }

    void OnStateChanged(GameManager.GameState state)
    {
        if (state != GameManager.GameState.Paused)
        {
            weaponList?.ClearSelection();
            itemList?.ClearSelection();
            hoverPanel?.Hide();
            selectedPanel?.Hide();
        }
    }

    public void OnSelectionChanged(WeaponList from)
    {
        if (itemList != null) itemList.ClearSelection();
    }

    public void OnSelectionChanged(ItemList from)
    {
        if (weaponList != null) weaponList.ClearSelection();
    }

    public void OnWeaponHover(WeaponResource weapon)
    {
        hoverPanel?.Show(weapon);
    }

    public void OnWeaponSelected(WeaponResource weapon)
    {
        selectedPanel?.Show(weapon);
    }

    public void OnWeaponDeselected()
    {
        selectedPanel?.Hide();
    }

    public void OnItemHover()
    {
        hoverPanel?.Hide();
    }

    public void OnHoverExit()
    {
        hoverPanel?.Hide();
    }
}
