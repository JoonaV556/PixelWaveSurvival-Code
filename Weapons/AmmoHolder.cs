using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmunitionHolder
{
    // public class AmmunitionType
    // {
    //     int currentAmmo;
    // }

    // Tracks ammo count for a player or a character

    // For testing - Add some intial ammo to the player

    public Dictionary<AmmunitionType, int> ammunition;

    public AmmunitionHolder()
    {
        ammunition = new Dictionary<AmmunitionType, int>
        {
            { AmmunitionType.Pistol, 100 },
            { AmmunitionType.SMG, 100 },
            { AmmunitionType.Shotgun, 100 }
        };
    }
}
