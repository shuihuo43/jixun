using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebuffIcon : MonoBehaviour
{
    public Dictionary<DebuffType, Sprite> spriteDict = new();

    [Serializable]
    public struct Entry
    {
        public DebuffType type;
        public Sprite sprite;
    }

    [SerializeField] private Entry[] entries;

    void Awake()
    {
        foreach (var e in entries)
            spriteDict[e.type] = e.sprite;
    }

    public void SetDebuff(DebuffType type)
    {
        var img = GetComponent<Image>();
        if (img != null && spriteDict.TryGetValue(type, out var sp))
            img.sprite = sp;
    }
}
