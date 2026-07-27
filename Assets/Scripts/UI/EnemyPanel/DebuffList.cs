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

        // 清旧
        foreach (var i in icons) Destroy(i);
        icons.Clear();

        // 按层数生成
        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(iconPrefab, transform);
            obj.GetComponent<DebuffIcon>()?.SetDebuff(type);
            obj.GetComponent<RectTransform>().sizeDelta = iconSize;
            icons.Add(obj);
        }
    }
}
