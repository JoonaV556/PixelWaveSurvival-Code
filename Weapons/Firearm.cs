using System;
using System.Collections;
using UnityEngine;

public enum FireMode
{
    Semi,
    FullAuto,
    Burst
}

public enum AmmunitionType
{
    Pistol,
    Shotgun,
    SMG
}

public class Firearm : MonoBehaviour
{
    // Handles gun firing logic

    // Fetches firearm data from firearm scriptable objects

    // Requires AmmunitionHolder for reloads and ammo tracking

    public static Action<Firearm> OnShotFired;
    public static Action<Firearm> OnDryFire;


    int currentAmmo; // Ammo in current magazine

    #region Weapon-Specific Properties
    int maxAmmo;
    int burstFireCount; // Number of rounds in a burst (burst will fire less than this if ammo is low)

    float fireRate; // Rounds per minute
    float reloadTime; // Time to reload the weapon in seconds
    float burstFireRate; // Rounds per minute for burst fire mode

    FireMode fireMode; // Current fire mode
    FireMode defaultFireMode; // Firemode after init
    FireMode[] possibleFireModes; // Possible fire modes on this gun

    AmmunitionType ammunitionType;
    #endregion

    bool initialized = false;

    bool tryingToFire = false; // Is the player trying to fire, according to input?

    bool reloading = false;

    bool firingFullAuto = false;

    bool firingBurst = false;

    Coroutine fullAutoRoutine;
    Coroutine reloadRoutine;

    AmmunitionHolder ammunitionHolder;

    // Remove later
    private void DebugInit()
    {
        currentAmmo = 50;
        maxAmmo = 50;
        possibleFireModes = new FireMode[] { FireMode.Semi, FireMode.FullAuto, FireMode.Burst };
        burstFireCount = 3;
        burstFireRate = 900;
        fireMode = FireMode.FullAuto;
        ammunitionType = AmmunitionType.SMG;
        ammunitionHolder = new AmmunitionHolder();
        fireRate = 700;
        reloadTime = 1f;
        initialized = true;
    }

    private void OnEnable()
    {
        PlayerInput.OnMainAttackPressed += OnMainAttackPressed;
        PlayerInput.OnMainAttackReleased += OnMainAttackReleased;
        PlayerInput.OnReload += OnReloadPressed;
        PlayerInput.OnSwitchFireMode += OnSwitchFireModePressed;

        // Allows testing without weapon data
        DebugInit();
    }

    private void OnDisable()
    {
        PlayerInput.OnMainAttackPressed -= OnMainAttackPressed;
        PlayerInput.OnMainAttackReleased -= OnMainAttackReleased;
        PlayerInput.OnReload -= OnReloadPressed;
        PlayerInput.OnSwitchFireMode -= OnSwitchFireModePressed;

        if (fullAutoRoutine != null)
        {
            StopCoroutine(fullAutoRoutine);
        }
        if (reloadRoutine != null)
        {
            StopCoroutine(reloadRoutine);
        }
    }

    private void OnReloadPressed()
    {
        if (CanReload())
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private void OnSwitchFireModePressed()
    {
        var canSwitch = !firingFullAuto;
        SwitchFireMode();
    }

    private void OnMainAttackPressed()
    {

        tryingToFire = true;

        switch (fireMode)
        {
            case FireMode.Semi:
                var canFire = AmmoLeftInMagazine() && !firingFullAuto && !reloading && !firingBurst;
                if (canFire)
                {
                    Fire();
                }
                else
                {
                    DryFire();
                }
                break;

            case FireMode.FullAuto:
                // Start firing full auto if we are not already firing full auto
                var canStart = AmmoLeftInMagazine() && !firingFullAuto && !reloading;
                if (canStart)
                {
                    //print("Starting full auto");
                    firingFullAuto = true;
                    fullAutoRoutine = StartCoroutine(FullAutoCoroutine());
                }
                else
                {
                    DryFire();
                }
                break;

            case FireMode.Burst:
                var canStartBurst = AmmoLeftInMagazine() && !firingBurst && !reloading && !firingFullAuto;
                if (canStartBurst)
                {
                    firingBurst = true;
                    StartCoroutine(BurstCoroutine(burstFireCount));
                }
                else
                {
                    DryFire();
                }
                break;
        }
    }

    private void OnMainAttackReleased()
    {
        tryingToFire = false;
    }

    // Initializes gun data. Gun cannot be used until initialized, unless using debug init
    private void Initialize()
    {
        // Fetch weapon data from the equipped weapon
        fireMode = defaultFireMode;

        initialized = true;
    }

    private void DryFire()
    {
        OnDryFire?.Invoke(this);
    }

    private void Fire()
    {
        if (!initialized) return;

        // Fires a single round
        OnShotFired?.Invoke(this);
        currentAmmo--;
        //print("Fired a round");
    }

    // Rounds per minute x : 700
    // Rounds per second  y : x / 60
    // Time between each round : 1 / y
    private IEnumerator FullAutoCoroutine()
    {
        // Cache wait for preventing unnecessary garbage
        var wait = new WaitForSeconds(1 / (fireRate / 60));

        while (true)
        {
            if (!tryingToFire)
            {
                // Stop firing if the player is not trying to fire
                firingFullAuto = false;
                yield break;
            }

            if (AmmoLeftInMagazine())
            {
                // Fire and wait for the next shot
                Fire();
                yield return wait;
            }
            else
            {
                // No ammo - dryfire and stop auto
                DryFire();
                firingFullAuto = false;
                yield break;
            }
        }
    }

    private IEnumerator BurstCoroutine(int burstCount)
    {
        // Cache wait for preventing unnecessary garbage
        var wait = new WaitForSeconds(1 / (burstFireRate / 60));
        int shotsFired = 0;

        while (true)
        {
            if (shotsFired == burstCount)
            {
                // Burst is done
                firingBurst = false;
                yield break;
            }

            if (AmmoLeftInMagazine())
            {
                // Fire and wait for the next shot
                Fire();
                shotsFired++;
                yield return wait;
            }
            else
            {
                // No ammo - dryfire and stop burst
                DryFire();
                firingBurst = false;
                yield break;
            }
        }
    }

    // If reload animations are used, waitforseconds should be equal to animation length 
    private IEnumerator ReloadRoutine()
    {
        // Start reloading
        reloading = true;
        yield return new WaitForSeconds(reloadTime); // Delay to simulate reload time
        Reload();

        // Done reloading
        reloading = false;
        reloadPending = false;
        yield break;
    }

    private bool CanReload()
    {
        return !firingFullAuto && !firingBurst && !reloading && ammunitionHolder.ammunition[ammunitionType] > 0;
    }

    private bool AmmoLeftInMagazine()
    {
        if (!initialized) return false;

        return currentAmmo > 0;
    }

    private void Reload()
    {
        if (!initialized) return;

        var hasEnoughAmmo = ammunitionHolder.ammunition[ammunitionType] >= maxAmmo;

        int requiredAmmo = maxAmmo - currentAmmo;

        if (hasEnoughAmmo)
        {
            // Reload to max ammo
            ammunitionHolder.ammunition[ammunitionType] -= requiredAmmo;
            currentAmmo = maxAmmo;
            // print("Reloaded full mag. Ammo left in inventory: " + ammunitionHolder.ammunition[ammunitionType]);
        }
        else
        {
            // Reload partial mag
            currentAmmo += ammunitionHolder.ammunition[ammunitionType];
            ammunitionHolder.ammunition[ammunitionType] = 0;
            // print("Reloaded partial mag. Ammo left in inventory: " + ammunitionHolder.ammunition[ammunitionType]);
        }
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
