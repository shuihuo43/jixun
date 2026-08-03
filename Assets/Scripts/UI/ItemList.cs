using UnityEngine;
using UnityEngine.UI;

public class ItemList : MonoBehaviour
{
    [SerializeField] private GameObject containerPrefab;
    public PlayerBag bag;

    private ItemContainer[] containers;
    private ItemContainer selected;
    private Player player;

    void Start()
    {
        player = FindObjectOfType<Player>();
        var items = player?.PlayerResource?.runSpawnItems;

        containers = new ItemContainer[5];
        for (int i = 0; i < 5; i++)
        {
            var obj = Instantiate(containerPrefab, transform);
            var c = obj.GetComponent<ItemContainer>();
            var item = items != null && i < items.Length ? items[i] : null;
            c.Refresh(item, this);
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

        var (slot, fill) = player.GetItemProgress();
        for (int i = 0; i < containers.Length; i++)
            containers[i]?.SetProcessFill(i == slot ? fill : 0f);
    }

    void OnClick(ItemContainer c, int idx)
    {
        if (GameManager.Instance?.CurrentState != GameManager.GameState.Paused) return;

        if (selected == null)
        {
            selected = c;
            c.SetHighlight(true);
            bag?.OnSelectionChanged(this);
        }
        else if (selected == c)
        {
            selected = null;
            c.SetHighlight(false);
        }
        else
        {
            var items = FindObjectOfType<Player>()?.PlayerResource?.runSpawnItems;
            if (items == null) return;

            if (items.Length < 5)
            {
                var pr = FindObjectOfType<Player>().PlayerResource;
                var resized = new ItemResource[5];
                for (int i = 0; i < items.Length; i++) resized[i] = items[i];
                pr.runSpawnItems = resized;
                items = resized;
            }

            int selIdx = System.Array.IndexOf(containers, selected);

            (items[selIdx], items[idx]) = (items[idx], items[selIdx]);
            containers[selIdx].Refresh(items[selIdx], this);
            containers[idx].Refresh(items[idx], this);
            selected.SetHighlight(false);
            selected = null;
        }
    }

    public void ClearSelection()
    {
        if (selected != null) { selected.SetHighlight(false); selected = null; }
    }

    public void OnHoverItem() => bag?.OnItemHover();
    public void OnHoverExit() => bag?.OnHoverExit();
}
