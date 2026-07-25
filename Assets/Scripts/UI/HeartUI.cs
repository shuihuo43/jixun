using System;
using UnityEngine;
using UnityEngine.UI;

public class HeartUI : MonoBehaviour
{
    [SerializeField] private Image heartImage;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite halfHeart;
    [SerializeField] private Sprite emptyHeart;

    public Action OnHeartChanged;

    public void SetState(int hp)
    {
        heartImage.sprite = hp >= 2 ? fullHeart : hp == 1 ? halfHeart : emptyHeart;
    }
}
