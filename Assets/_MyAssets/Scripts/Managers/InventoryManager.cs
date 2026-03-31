using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // ============================
    // Serialized Fields
    // ============================

    public Transform rightHand;
    public AudioSource pickupAudioSource;
    public AudioClip pickupAudioClip;

    // ============================
    // Weapon State
    // ============================

    public WeaponHook currentWeaponHook;
    public List<Item> pickedUpItems = new List<Item>();
    public List<WeaponItem> allWeapons;
    Dictionary<WeaponItem, WeaponHook> weaponsDict = new Dictionary<WeaponItem, WeaponHook>();

    // ============================
    // References
    // ============================

    Controller playerController;
    PassiveItem currentPassiveItem;

    // ============================
    // Properties
    // ============================

    public WeaponItem currentWeapon
    {
        get
        {
            if (currentWeaponHook == null)
                return null;
            return currentWeaponHook.baseItem;
        }
    }

    // ============================
    // Lifecycle
    // ============================

    private void Start()
    {
        pickupAudioSource = GetComponent<AudioSource>();
        playerController = GetComponent<Controller>();

        if (allWeapons.Count > 0)
            LoadWeapon(allWeapons[0]);
    }

    // ============================
    // Public API — Pickup
    // ============================

    public void PickUpItem(Item item)
    {
        if (item is WeaponItem weapon)
        {
            pickedUpItems.Add(item);

            if (pickupAudioSource != null && pickupAudioClip != null)
            {
                pickupAudioSource.clip = pickupAudioClip;
                pickupAudioSource.Play();
            }

            if (!allWeapons.Contains(weapon))
            {
                allWeapons.Add(weapon);
                LoadWeapon(weapon);
            }
        }
        else if (item is PassiveItem)
        {
            pickedUpItems.Add(item);
        }
    }

    // ============================
    // Public API — Weapon Switching
    // ============================

    public void SwitchWeapon()
    {
        if (allWeapons.Count <= 0)
            return;

        int weaponIndex = 0;
        if (currentWeapon != null && allWeapons.Contains(currentWeapon))
            weaponIndex = allWeapons.IndexOf(currentWeapon);

        weaponIndex = (weaponIndex + 1) % allWeapons.Count;
        LoadWeapon(allWeapons[weaponIndex]);
    }

    // ============================
    // Public API — Item Loading
    // ============================

    public void LoadItem(Item targetItem)
    {
        if (targetItem is WeaponItem weapon)
        {
            LoadWeapon(weapon);
        }
        else if (targetItem is PassiveItem passive && playerController != null)
        {
            if (currentPassiveItem != null)
                currentPassiveItem.OnUnEquip(playerController);

            passive.OnEquip(playerController);
            currentPassiveItem = passive;
        }
    }

    public void LoadWeapon(WeaponItem weaponItem)
    {
        if (currentWeaponHook != null)
            currentWeaponHook.gameObject.SetActive(false);

        if (weaponsDict.ContainsKey(weaponItem))
        {
            weaponsDict.TryGetValue(weaponItem, out currentWeaponHook);
        }
        else
        {
            GameObject go = Instantiate(weaponItem.prefab);
            go.transform.parent = rightHand;
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;
            currentWeaponHook = go.GetComponentInChildren<WeaponHook>();
            weaponsDict.Add(weaponItem, currentWeaponHook);
        }

        currentWeaponHook.gameObject.SetActive(true);
        currentWeaponHook.Init(weaponItem);
    }
}
