using System.Collections.Generic;
using UnityEngine;

public class DebuffList : MonoBehaviour
{
    [SerializeField] private GameObject iconPrefab;
    [SerializeField] private Vector2 iconSize = new Vector2(2, 2);

    private List<GameObject> icons = new();
    private int lastCount = -1;

    public void SetCount(int count, DebuffType type)
    {
        if (count == lastCount) return;
        lastCount = count;

        foreach (var i in icons) Destroy(i);
        icons.Clear();

        if (count <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(iconPrefab, transform);
            obj.GetComponent<DebuffIcon>()?.SetDebuff(type);
            obj.GetComponent<RectTransform>().sizeDelta = iconSize;
            icons.Add(obj);
        }
    }
}
