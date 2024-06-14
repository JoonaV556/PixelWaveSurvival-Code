/*

Interface for controlling various types of weapons with similar input methods.

Each weapon type (gun, melee, throwable, etc.) has its own implementation of this interface.

This method of input handling is required for the weapon slot system to work.

WeaponSlotController has list of available weapons player can use, but only one can be active at a time.

When weapon-related input is received, WeaponSlotController forwards it to the active weapon using this interface.

*/

/// <summary>
/// Interface for controlling various types of weapons with similar input methods.
/// </summary>
/// <remarks>Obviously not all weapon types need to implement logic for these functions. In that case, justs leave them empty</remarks>
public interface IWeapon
{
    public void OnReloadPressed()
    {

    }

    public void OnSwitchFireModePressed()
    {

    }

    public void OnMainAttackPressed()
    {

    }

    public void OnMainAttackReleased()
    {

    }
}
