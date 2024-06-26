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

/// <summary>
/// Universal firearm script for all kinds of guns/firearms
/// </summary>
public class Firearm : MonoBehaviour, IWeapon
{
    /*

    Universal firearm script for all kinds of guns/firearms

    Features:
    - Firemodes (Semi, Full auto, Burst)
    - Reloads (Partial and full reloads)
    - Modular/swappable ammo tracking (Easy to implement custom ammo tracking)
    - Easily swappable input sources 
    - Fire mode switching
    - Dry fire (single hammer hit after emptying the magazine)
    - Events for shot fired and dry fire
    - Supports fetching weapon data from custom sources
    - Optional bullet spread
    
    Rounds per minute = (x)
    Rounds per second  = (y) = x / 60
    Time between each round = 1 / y

    */

    public static Action<Firearm> OnShotFired;
    public static Action<Firearm> OnDryFire;

    public Transform ProjectileSpawnPoint; // Where the projectile will spawn

    public bool EnableAutoReload = true; // Automatically reload when out of ammo

    [Tooltip("If true, ammo is not removed from ammoHolder on reload. Reload still necessary. ")]
    public bool InfiniteAmmo = false; // Infinite ammo for testing

    int ammoInGun; // Ammo in current magazine

    #region Weapon-Specific Properties
    // Fetch these values from other sources in Initialize()
    public GameObject BulletPrefab; // Prefab for the bullet fired by this weapon
    int maxAmmo;
    int burstFireCount; // Number of rounds in a burst (burst will fire less than this if ammo is low)

    float fireRate; // Rounds per minute
    float reloadTime; // Time to reload the weapon in seconds
    float burstFireRate; // Rounds per minute for burst fire mode
    public float bulletSpawnVelocity = 10f;

    // Fire mode properties
    FireMode fireMode; // Current fire mode
    FireMode defaultFireMode; // Firemode after init
    FireMode[] possibleFireModes; // Possible fire modes on this gun

    AmmunitionType ammunitionType;

    // Bullet spread properties
    public bool ApplySpread = true;
    float[] bulletSpread;
    int spreadIndex = 0;
    #endregion

    bool initialized = false;

    bool tryingToFire = false; // Is the player trying to fire, according to input?

    bool reloading = false;

    bool firingFullAuto = false;

    bool firingBurst = false;

    Coroutine fullAutoRoutine;
    Coroutine reloadRoutine;

    AmmunitionHolder ammunitionHolder; // Swap this property (+ any references to it) for custom ammo handling

    // Remove later - Creates simulated weapon data
    public float spreadDegreesMin = -5f;
    public float spreadDegreesMax = 5f;
    public float spreadMultiplier = 1f;
    private void DebugInit()
    {
        ammoInGun = 50;
        maxAmmo = 50;
        possibleFireModes = new FireMode[] { FireMode.Semi, FireMode.FullAuto, FireMode.Burst };
        burstFireCount = 3;
        burstFireRate = 900;
        fireMode = FireMode.FullAuto;
        ammunitionType = AmmunitionType.SMG;
        ammunitionHolder = new AmmunitionHolder();
        fireRate = 700;
        reloadTime = 1f;
        bulletSpread = GetRandomizedBulletSpread(spreadDegreesMin, spreadDegreesMax, 30, spreadMultiplier);

        initialized = true;
    }

    /// <summary>
    /// Randomizes the bullet spread with given parameters
    /// </summary>
    public void DebugRandomizeSpread()
    {
        bulletSpread = GetRandomizedBulletSpread(spreadDegreesMin, spreadDegreesMax, 30, spreadMultiplier);
    }

    private void OnEnable()
    {
        PlayerInput.OnMainAttackPressed += MainAttackPressed;
        PlayerInput.OnMainAttackReleased += MainAttackReleased;
        PlayerInput.OnReload += ReloadPressed;
        PlayerInput.OnSwitchFireMode += SwitchFireModePressed;

        // Allows testing without weapon data
        DebugInit();
    }

    private void OnDisable()
    {
        PlayerInput.OnMainAttackPressed -= MainAttackPressed;
        PlayerInput.OnMainAttackReleased -= MainAttackReleased;
        PlayerInput.OnReload -= ReloadPressed;
        PlayerInput.OnSwitchFireMode -= SwitchFireModePressed;

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
        // Handle auto reload
        HandleAutoReload();
    }

    private void HandleAutoReload()
    {
        if (!EnableAutoReload) return;
        bool shouldReload = ammoInGun == 0;
        if (shouldReload && CanReload())
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    #region InputReactions
    private void ReloadPressed()
    {
        if (CanReload())
        {
            StartCoroutine(ReloadRoutine());
        }
    }

    private void SwitchFireModePressed()
    {
        var canSwitch = !firingFullAuto;
        SwitchFireMode();
    }

    private void MainAttackPressed()
    {

        tryingToFire = true;
        var shouldDryFire = !AmmoLeftInMagazine() && !reloading && !firingFullAuto && !firingBurst;

        switch (fireMode)
        {
            case FireMode.Semi:
                var canFire = AmmoLeftInMagazine() && !firingFullAuto && !reloading && !firingBurst;
                if (canFire)
                {
                    Fire();
                }
                if (shouldDryFire)
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
                if (shouldDryFire)
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
                if (shouldDryFire)
                {
                    DryFire();
                }
                break;
        }
    }

    private void MainAttackReleased()
    {
        tryingToFire = false;
    }
    #endregion

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
        LaunchProjectile();

        OnShotFired?.Invoke(this);
        ammoInGun--;
        //print("Fired a round");
    }

    private void LaunchProjectile()
    {
        // TODO - Implement pooling to reduce GC

        // Create new projectile at barrel end
        var projectile = Instantiate(BulletPrefab, ProjectileSpawnPoint.position, Quaternion.identity);

        // Rotate towards barrel pointing direction
        projectile.transform.right = ProjectileSpawnPoint.right;

        // Apply optional bullet spread
        if (ApplySpread)
        {
            projectile.transform.Rotate(0, 0, bulletSpread[spreadIndex]);
            spreadIndex++;
            spreadIndex = spreadIndex % bulletSpread.Length; // Start from beginning if we reach the end of spread array
        }

        // Make the projectile fly towards barrel pointing direction
        projectile.GetComponent<Rigidbody2D>().velocity = projectile.transform.right.normalized * bulletSpawnVelocity;
    }

    /// <summary>
    /// Returns true if ammo holder has atleast 1 bullet to reload with and weapon is not currectly firing or reloading
    /// </summary>
    /// <returns></returns>
    private bool CanReload()
    {
        return !firingFullAuto && !firingBurst && !reloading && ammunitionHolder.ammunition[ammunitionType] > 0;
    }

    private bool AmmoLeftInMagazine()
    {
        if (!initialized) return false;

        return ammoInGun > 0;
    }

    private void Reload()
    {
        if (!initialized) return;

        var hasEnoughAmmo = ammunitionHolder.ammunition[ammunitionType] >= maxAmmo;

        int requiredAmmo = maxAmmo - ammoInGun;

        if (InfiniteAmmo)
        {
            ammoInGun = maxAmmo;
            return;
        }

        if (hasEnoughAmmo)
        {
            // Reload to max ammo
            ammunitionHolder.ammunition[ammunitionType] -= requiredAmmo;
            ammoInGun = maxAmmo;
            // print("Reloaded full mag. Ammo left in inventory: " + ammunitionHolder.ammunition[ammunitionType]);
        }
        else
        {
            // Reload partial mag
            ammoInGun += ammunitionHolder.ammunition[ammunitionType];
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
        yield break;
    }

    /// <summary>
    /// Returns an array of randomized bullet spread values in degrees.
    /// </summary>
    /// <param name="length">Length of the array</param>
    /// <param name="multiplier">Optional multiplier applied to each spread degree</param>
    /// <returns>Array of spread floats</returns>
    private float[] GetRandomizedBulletSpread(
        float degreesMin,
        float degreesMax,
        int length = 30,
        float multiplier = 1f
        )
    {
        var spread = new float[length];

        for (int i = 0; i < length; i++)
        {
            var newSpread = UnityEngine.Random.Range(degreesMin, degreesMax) * multiplier;
            spread[i] = newSpread;
        }
        return spread;
    }

    #region IWeaponInput
    public void OnReloadPressed()
    {
        throw new NotImplementedException();
    }

    public void OnSwitchFireModePressed()
    {
        throw new NotImplementedException();
    }

    public void OnMainAttackPressed()
    {
        throw new NotImplementedException();
    }

    public void OnMainAttackReleased()
    {
        throw new NotImplementedException();
    }
    #endregion
}
