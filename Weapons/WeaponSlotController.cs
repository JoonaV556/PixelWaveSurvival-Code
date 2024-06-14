using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlotController : MonoBehaviour
{
    // Holds list of weapon slots
    // Sends weapon-related input to active weapon

    [HideInInspector]
    public List<WeaponSlot> weaponSlots;

    public WeaponSlot activeSlot;

    public PlayerInput PlayerInput;

    public int maxSlots = 9;

    public int activeSlotIndex;

    int startSlotIndex = 0;

    private void Awake()
    {
        weaponSlots = new List<WeaponSlot>();

        // Get all weapons on same object
        var weaponsOnThisObject = GetComponents<IWeapon>();
        // Create weapon slots for each weapon
        foreach (var weapon in weaponsOnThisObject)
        {
            weaponSlots.Add(new WeaponSlot(weapon, true));
        }

        activeSlot = weaponSlots[startSlotIndex];
        activeSlotIndex = startSlotIndex;
    }

    private void OnEnable()
    {
        PlayerInput.OnWeaponSlotSwitched += OnWeaponSlotSwitched;
        PlayerInput.OnMainAttackPressed += OnMainAttackPressed;
        PlayerInput.OnMainAttackReleased += OnMainAttackReleased;
        PlayerInput.OnReload += OnReloadPressed;
        PlayerInput.OnSwitchFireMode += OnSwitchFireModePressed;
    }

    private void OnDisable()
    {
        PlayerInput.OnWeaponSlotSwitched -= OnWeaponSlotSwitched;
        PlayerInput.OnMainAttackPressed += OnMainAttackPressed;
        PlayerInput.OnMainAttackReleased += OnMainAttackReleased;
        PlayerInput.OnReload += OnReloadPressed;
        PlayerInput.OnSwitchFireMode += OnSwitchFireModePressed;
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

    #region Active Weapon Input Handling
    private IWeapon ActiveWeapon() => activeSlot.heldWeapon;
    public void OnReloadPressed()
    {
        ActiveWeapon().OnReloadPressed();
    }
    public void OnSwitchFireModePressed()
    {
        ActiveWeapon().OnSwitchFireModePressed();
    }
    public void OnMainAttackPressed()
    {
        ActiveWeapon().OnMainAttackPressed();
    }
    public void OnMainAttackReleased()
    {
        ActiveWeapon().OnMainAttackReleased();
    }
    #endregion
}

[Serializable]
public class WeaponSlot
{
    public bool canSwitchHeldWeapon = true;
    public IWeapon heldWeapon;

    public WeaponSlot(IWeapon weapon = null, bool canSwitchWeapon = true)
    {
        heldWeapon = weapon;
        canSwitchHeldWeapon = canSwitchWeapon;
    }

    public void SwitchWeapon(IWeapon newWeapon)
    {
        if (canSwitchHeldWeapon)
        {
            heldWeapon = newWeapon;
        }
    }
}

public class Fists : IWeapon
{
    // TODO - Refactor when weapons are implemented
}
