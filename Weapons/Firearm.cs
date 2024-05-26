using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    // Requires AmmunitionHolder for reloads and ammo tracking

    public static Action<Firearm> OnShotFired;

    int maxAmmo;
    int currentAmmo; // Ammo in current magazine
    float fireRate; // Rounds per minute

    float reloadTime; // Time to reload the weapon in seconds

    FireMode fireMode; // Current fire mode

    FireMode defaultFireMode; // Firemode after init

    FireMode[] possibleFireModes; // Possible fire modes on this gun

    bool initialized = false;

    bool tryingToFire = false;

    bool autoReload = true; // TODO - make this a setting in some general place, then notify firearm when it is changed

    bool reloading = false;

    bool reloadPending = false;

    bool firingFullAuto = false;

    Coroutine fullAutoRoutine;
    Coroutine reloadRoutine;

    private void OnEnable()
    {
        PlayerInput.MainAttackPressed += OnMainAttackPressed;
        PlayerInput.MainAttackReleased += OnMainAttackReleased;

        // Allows testing without weapon data
        DebugInit();
    }

    private void DebugInit()
    {
        currentAmmo = 100;
        fireMode = FireMode.FullAuto;
        fireRate = 700;
        reloadTime = 1f;
        initialized = true;
    }

    private void OnDisable()
    {
        PlayerInput.MainAttackPressed -= OnMainAttackPressed;
        PlayerInput.MainAttackReleased -= OnMainAttackReleased;

        if (fullAutoRoutine != null)
        {
            StopCoroutine(fullAutoRoutine);
        }
        if (reloadRoutine != null)
        {
            StopCoroutine(reloadRoutine);
        }
    }

    private void Update()
    {
        if (reloadPending && !reloading)
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private void OnMainAttackPressed()
    {

        tryingToFire = true;

        switch (fireMode)
        {
            case FireMode.Semi:

                break;

            case FireMode.FullAuto:
                // Start firing full auto if we are not already firing full auto
                if (fullAutoRoutine == null)
                {
                    print("Starting full auto");
                    firingFullAuto = true;
                    fullAutoRoutine = StartCoroutine(FullAutoCoroutine());
                }
                break;

            case FireMode.Burst:

                break;
        }
    }

    private void OnMainAttackReleased()
    {
        tryingToFire = false;
    }

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
        OnShotFired?.Invoke(this);
        currentAmmo--;
        print("Fired a round");
    }

    // Rounds per minute x : 700
    // Rounds per second  y : x / 60
    // Time between each round : 1 / y
    private IEnumerator FullAutoCoroutine()
    {
        while (true)
        {
            // Stop the coroutine if the player is no longer trying to fire
            if (!tryingToFire)
            {
                firingFullAuto = false;
                fullAutoRoutine = null;
                yield break;
            }

            if (HasAmmo())
            {
                // Fire and wait for the next shot
                Fire();
                //yield return new WaitForSeconds(1 / (fireRate / 60));
                yield return new WaitForSeconds(1f / (fireRate / 60f));
            }
            // else
            // {
            //     // No ammo - so reload and stop firing
            //     reloadPending = true;
            //     firingFullAuto = false;
            //     yield break;
            // }
        }
    }

    // If reload animations are used, waitforseconds should be equal to animation length 
    private IEnumerator ReloadRoutine()
    {
        reloading = true;
        yield return new WaitForSeconds(reloadTime);
        Reload();
        reloading = false;
        reloadPending = false;
        //reloadRoutine = null;
        yield break;
    }

    private bool HasAmmo()
    {
        if (!initialized) return false;

        return currentAmmo > 0;
    }

    private void AttemptReload()
    {
        if (!initialized) return;


    }


    private void Reload()
    {
        if (!initialized) return;



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
