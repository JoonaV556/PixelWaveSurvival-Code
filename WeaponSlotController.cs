using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlotController : MonoBehaviour
{
    public List<WeaponSlot> weaponSlots;

    public WeaponSlot activeSlot;

    public int maxSlots = 9;

    public int activeSlotIndex;

    int startSlotIndex = 0;

    private void Awake()
    {
        weaponSlots = new List<WeaponSlot>();

        // Add first slot for fists 
        var fistSlot = new WeaponSlot(new Fists(), false);
        weaponSlots.Add(fistSlot);

        // Add two more slots for weapons
        for (int i = 1; i <= 2; i++)
        {
            weaponSlots.Add(new WeaponSlot());
        }

        activeSlot = weaponSlots[startSlotIndex];
        activeSlotIndex = startSlotIndex;
    }

    private void OnEnable()
    {
        PlayerInput.OnWeaponSlotSwitched += OnWeaponSlotSwitched;
    }

    private void OnDisable()
    {
        PlayerInput.OnWeaponSlotSwitched -= OnWeaponSlotSwitched;
    }

    private void OnWeaponSlotSwitched(int numberKeyPressed)
    {
        var index = numberKeyPressed - 1;
        bool canSwitchToSlot = index >= 0 && index < weaponSlots.Count;
        if (canSwitchToSlot)
        {
            activeSlot = weaponSlots[index];
            activeSlotIndex = weaponSlots.IndexOf(activeSlot);
            Debug.Log("Switching to slot " + index);
        }
    }

    public bool AddSlot()
    {
        bool canAdd = weaponSlots.Count < maxSlots;

        if (canAdd)
        {
            weaponSlots.Add(new WeaponSlot());
            return true;
        }
        return false;
    }
}

[Serializable]
public class WeaponSlot
{
    public bool canSwitchHeldWeapon = true;
    public Weapon heldWeapon;

    public WeaponSlot(Weapon weapon = null, bool canSwitchWeapon = true)
    {
        heldWeapon = weapon;
        canSwitchHeldWeapon = canSwitchWeapon;
    }

    public void SwitchWeapon(Weapon newWeapon)
    {
        if (canSwitchHeldWeapon)
        {
            heldWeapon = newWeapon;
        }
    }
}

[Serializable]
public class Weapon
{
    // TODO - Refactor when weapons are implemented
}

public class Fists : Weapon
{
    // TODO - Refactor when weapons are implemented
}
