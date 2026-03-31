using UnityEngine;

[CreateAssetMenu(menuName = "Items/Weapon Item")]
public class WeaponItem : Item
{
    public GameObject preFab;
    public int magazineAmmo = 40;
    public int damageValue = 20;
    public float fireRate = 0.1f;
    public float weaponSpread = .2f;
    public bool canMoveWithWeapon;
}
