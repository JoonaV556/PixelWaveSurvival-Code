using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireMode
{
    Semi,
    FullAuto,
    Burst
}



public class Firearm : MonoBehaviour
{
    // Handles gun firing logic

    // Fetches firearm data from firearm scriptable objects

    // Requires Ammoholder for reloads and ammo tracking

    int maxAmmo;
    int currentAmmo;
    float fireRate; // Rounds per minute

    FireMode fireMode; // Current fire mode

    FireMode defaultFireMode; // Firemode after init

    FireMode[] possibleFireModes; // Possible fire modes on this gun

    bool initialized = false;

    // Initializes gun data. Gun cannot be used until initialized
    private void Initialize()
    {
        // Fetch weapon data from the equipped weapon
        fireMode = defaultFireMode;

        initialized = true;
    }

    private void Fire()
    {
        if (!initialized) return;

        // Fires a single round
    }

    private void AttemptReload()
    {
        if (!initialized) return;


    }

    private void Reload()
    {

    }

    private void SwitchFireMode()
    {
        if (!initialized) return;

        // Switches the fire mode of the weapon
        int currentIndex = Array.IndexOf(possibleFireModes, fireMode);
        int nextIndex = (currentIndex + 1) % possibleFireModes.Length;
        fireMode = possibleFireModes[nextIndex];
    }
}
