using UnityEngine;

public class TreasureList : MonoBehaviour
{
    [SerializeField] private ModifierResource[] resources;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private ModifierTooltip tooltip;

    void Start()
    {
        foreach (var res in resources)
        {
            var parent = panelRoot != null ? panelRoot.transform : transform;
            var obj = Instantiate(buttonPrefab, parent);
            var tb = obj.GetComponent<TreasureButton>();
            if (tb != null)
            {
                tb.Init(res);
                tb.OnHover += r => tooltip?.Show(r, 1);
                tb.OnHoverExit += () => tooltip?.Hide();
            }
        }

        if (panelRoot != null) panelRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5) && panelRoot != null)
            panelRoot.SetActive(!panelRoot.activeSelf);
    }
}
