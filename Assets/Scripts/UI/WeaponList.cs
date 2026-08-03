using UnityEngine;
using UnityEngine.UI;

public class WeaponList : MonoBehaviour
{
    [SerializeField] private GameObject containerPrefab;
    public PlayerBag bag;

    private WeaponContainer[] containers;
    private WeaponContainer selected;
    private Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();
        var weapons = player?.PlayerResource?.WeaponResources;

        containers = new WeaponContainer[5];
        for (int i = 0; i < 5; i++)
        {
            var obj = Instantiate(containerPrefab, transform);
            var c = obj.GetComponent<WeaponContainer>();
            var weapon = weapons != null && i < weapons.Length ? weapons[i] : null;
            c.Refresh(weapon, this);
            int idx = i;
            c.OnClickEvent += () => OnClick(c, idx);
            var btn = c.GetComponent<Button>();
            if (btn != null) btn.onClick.AddListener(() => c.OnClick());
            containers[i] = c;
        }
    }

    void Update()
    {
        if (player == null || containers == null) return;

        var (slot, fill) = player.GetWeaponProgress();
        for (int i = 0; i < containers.Length; i++)
            containers[i]?.SetProcessFill(i == slot ? fill : 0f);
    }

    void OnClick(WeaponContainer c, int idx)
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Paused) return;

        if (selected == null)
        {
            selected = c;
            c.SetHighlight(true);
            bag?.OnSelectionChanged(this);
            bag?.OnWeaponSelected(c.Resource);
        }
        else if (selected == c)
        {
            selected = null;
            c.SetHighlight(false);
            bag?.OnWeaponDeselected();
        }
        else
        {
            var weapons = FindObjectOfType<Player>()?.PlayerResource?.WeaponResources;
            if (weapons == null) return;

            // 确保数组长度够 5
            if (weapons.Length < 5)
            {
                var pr = FindObjectOfType<Player>().PlayerResource;
                var resized = new WeaponResource[5];
                for (int i = 0; i < weapons.Length; i++) resized[i] = weapons[i];
                pr.WeaponResources = resized;
                weapons = resized;
            }

            int selIdx = System.Array.IndexOf(containers, selected);

            (weapons[selIdx], weapons[idx]) = (weapons[idx], weapons[selIdx]);
            containers[selIdx].Refresh(weapons[selIdx], this);
            containers[idx].Refresh(weapons[idx], this);
            selected.SetHighlight(false);
            selected = null;
            bag?.OnWeaponDeselected();
            OnHoverWeapon(weapons[idx]);
        }
    }

    public void ClearSelection()
    {
        if (selected != null) { selected.SetHighlight(false); selected = null; }
        bag?.OnWeaponDeselected();
    }

    public void OnHoverWeapon(WeaponResource w) => bag?.OnWeaponHover(w);
    public void OnHoverExit() => bag?.OnHoverExit();
}
