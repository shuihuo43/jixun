using UnityEngine;
using UnityEngine.UI;

public class ComboNumber : MonoBehaviour
{
    [SerializeField] private Sprite[] digits; // 0-9

    public void SetDigit(int d)
    {
        var img = GetComponent<Image>();
        if (img != null && digits != null && d >= 0 && d < digits.Length)
            img.sprite = digits[d];
    }
}
