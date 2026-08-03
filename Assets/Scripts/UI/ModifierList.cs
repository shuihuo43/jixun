using System.Collections.Generic;
using UnityEngine;

public class ModifierList : MonoBehaviour
{
    [SerializeField] private GameObject containerPrefab;
    [SerializeField] private ModifierTooltip tooltip;

    private List<ModifierContainer> containers = new();
    private Player player;
    private int lastCount;

    void Start()
    {
        player = FindObjectOfType<Player>();
        lastCount = -1;
    }

    void Update()
    {
        if (player?.PlayerResource?.modifiers == null) return;
        var mods = player.PlayerResource.modifiers;
        if (mods.Count == lastCount) return;
        lastCount = mods.Count;

        foreach (var c in containers) Destroy(c.gameObject);
        containers.Clear();

        var dict = new Dictionary<ModifierResource, int>();
        foreach (var m in mods)
            dict[m] = dict.TryGetValue(m, out int v) ? v + 1 : 1;

        foreach (var kv in dict)
        {
            var obj = Instantiate(containerPrefab, transform);
            var c = obj.GetComponent<ModifierContainer>();
            if (c != null)
            {
                c.Refresh(kv.Key, kv.Value);
                c.OnHover += OnContainerHover;
                c.OnHoverExit += OnContainerHoverExit;
                containers.Add(c);
            }
        }
    }

    void OnContainerHover(ModifierResource res, int count) => tooltip?.Show(res, count);
    void OnContainerHoverExit() => tooltip?.Hide();
}
