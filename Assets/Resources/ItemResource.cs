using UnityEngine;

[CreateAssetMenu(fileName = "ItemResource", menuName = "Resource/ItemResource")]
public class ItemResource : ScriptableObject
{
    public GameObject prefab;
    public Sprite icon;
}
