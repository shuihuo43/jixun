using UnityEngine;

[CreateAssetMenu(fileName = "WeaponResource", menuName = "Resource/WeaponResource")]
public class WeaponResource : ScriptableObject
{
    public enum ActionType
    {
        Claymore = 0, //kuo jian
        Scythe = 1, //lian dao
        Tow_knives = 2, //shuang dao
        Spear = 3, //chang qiang
        Scimitar = 4, //qu jian
        Handgun = 5, //quan tao
    }

    public string weaponName;
    [TextArea] public string description;
    public Sprite icon;
    public int AttackCount = 1;
    public ActionType AttackType = ActionType.Claymore;
    public float AttackAngleOffset = 0f;
    public DamageResource damageResource;
}
