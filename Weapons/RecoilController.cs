using System.Linq.Expressions;
using UnityEngine;

public class RecoilController : MonoBehaviour
{
    /*

    Applies vertical recoil for guns by rotating the gun sprite upwards when shot is fired

    Features:
    * vertical recoil upwards
        * Customizable properties:
            - KickPerShot: How many degrees the gun kicks upwards per shot fired
            - KickDegreesUpperLimit: How many degrees the gun can kick upwards at most. Relative to 0 degrees on z axis
            - RecoilSlerpAlpha: How fast gun kicks up
            - RecoilAlphaMultiplier: Optional multiplier for the alpha values
    * Gun returns back to original rotation after recoil
        * Customizable properties:
            - PullBackAlpha: How fast gun returns back to original rotation after recoil
            - PullbackAlphaMultiplier: Optional multiplier for the alpha values
            - PullDownDelayLength: How much time has to pass (in seconds) since last shot fired, before gun starts to pull back down to actual aim direction
    
    Predefined public parameters are tailored for MP5 or other small SMGs

    */

    public Transform RecoilPivot;

    public bool Enabled = true;

    [Tooltip("How many degrees the gun kicks upwards per shot fired.")]
    public float KickPerShot = 2f;
    [Tooltip("How many degrees the gun can kick upwards at most. Relative to 0 degrees on z axis.")]
    public float KickDegreesUpperLimit = 30f;

    [Tooltip("How fast gun kicks up")]
    [Range(0, 1)] public float RecoilSlerpAlpha = 0.99f;
    [Tooltip("How fast gun returns back to original rotation after recoil.")]
    [Range(0, 1)] public float PullBackAlpha = 0.28f;

    public bool UseRecoilMultiplier = true;
    public bool UsePullbackMultiplier = true;

    [Tooltip("Optional multiplier for the alpha values")]
    public float RecoilAlphaMultiplier = 180f;
    [Tooltip("Optional multiplier for the alpha values")]
    public float PullbackAlphaMultiplier = 180f;

    [Tooltip("How much time has to pass (in seconds) since last shot fired, before gun starts to pull back down to original rotation")]
    public float PullDownDelayLength = 0.1f;

    Vector3 targetRot = Vector3.zero; // Make private later
    LookSide lastLookSide = LookSide.Right;
    public bool EnablePulldown = true;
    public Vector3 currentRot = Vector3.zero;

    private void Start()
    {
        targetRot = RecoilPivot.localRotation.eulerAngles;
        lastLookSide = WeaponSpriteController.CurrentLookSide;
        // print("Lookside at start: " + WeaponSpriteController.CurrentLookSide.ToString());

        // Set initial rotation based on look side
        switch (WeaponSpriteController.CurrentLookSide)
        {
            case LookSide.Left:
                RecoilPivot.localEulerAngles = new Vector3(0f, 0f, 359.99f);
                break;
            case LookSide.Right:
                RecoilPivot.localEulerAngles = new Vector3(0f, 0f, 0f);
                break;
        }
    }

    private void OnEnable()
    {
        Firearm.OnShotFired += AddKick;
    }

    private void OnDisable()
    {
        Firearm.OnShotFired -= AddKick;
    }

    float pendingKick = 0f;
    float secondsBeforePulldown = 0f;
    private void AddKick(Firearm firearm)
    {
        secondsBeforePulldown = PullDownDelayLength;
        pendingKick += KickPerShot;
    }

    private void Update()
    {
        // Get current rotation for later use
        currentRot = RecoilPivot.transform.localRotation.eulerAngles;

        if (!Enabled)
        {
            return;
        }

        // Flip gun sprite z rotation if the look side has changed
        if (WeaponSpriteController.CurrentLookSide != lastLookSide)
        {
            lastLookSide = WeaponSpriteController.CurrentLookSide;
            float newZRot = (WeaponSpriteController.CurrentLookSide == LookSide.Left) ? 359.99f - currentRot.z : 0 + (359.99f - currentRot.z);
            RecoilPivot.localEulerAngles = new Vector3(currentRot.x, currentRot.y, newZRot);
            currentRot = RecoilPivot.transform.localRotation.eulerAngles; // Update current rotation
        }

        // Determine target rotation to slerp to
        UpdateTargetRotation();

        // Reduce cooldown if there is any
        if (secondsBeforePulldown > 0f)
        {
            secondsBeforePulldown = Mathf.Clamp(secondsBeforePulldown - Time.deltaTime, 0f, PullDownDelayLength);
        }

        float timedAlpha = GetTimedAlpha();

        // Apply rotation using slerp
        RecoilPivot.localEulerAngles = Vector3.Slerp(currentRot, targetRot, timedAlpha);

        // Debug 
        currentRot = RecoilPivot.transform.localRotation.eulerAngles;
        // print(currentRot.ToString());
    }

    private void UpdateTargetRotation()
    {
        if (secondsBeforePulldown == 0f) // No recoil left - start pulling down
        {
            // Recoil ended - Rotate gun back to aim direction
            if (EnablePulldown)
            {
                // Pulldown enabled, start rotating towards aim direction
                switch (WeaponSpriteController.CurrentLookSide)
                {
                    case LookSide.Left:
                        targetRot.z = 359.99f;
                        break;
                    case LookSide.Right:
                        targetRot.z = 0f;
                        break;
                }
            }
            else
            {
                // Pulldown disabled, keep z rotation as is
                targetRot.z = currentRot.z;
            }
        }
        else if (pendingKick > 0f) // Recoil is still pending - apply recoil
        {
            // Recoil is still pending - Rotate gun upwards based on look direction
            var currentLookside = WeaponSpriteController.CurrentLookSide;
            targetRot = new Vector3(currentRot.x, currentRot.y, currentRot.z);
            // print("Target rot before recoil: " + targetRot.z);

            // Add recoil based on look side
            switch (currentLookside)
            {
                case LookSide.Left:
                    targetRot.z -= pendingKick;
                    break;
                case LookSide.Right:
                    targetRot.z += pendingKick;
                    break;

            }

            // Reset pending recoil kick
            pendingKick = 0f;

            // Clamp rotation to prevent gun from rotating too much
            float zClampMin = (currentLookside == LookSide.Left) ? 359.99f - KickDegreesUpperLimit : 0f;
            float zClampMax = (currentLookside == LookSide.Left) ? 359.99f : KickDegreesUpperLimit;
            targetRot.z = Mathf.Clamp(targetRot.z, zClampMin, zClampMax);
        }
        else // Gun is already pointing at aim direction - keep it there
        {
            // No pending recoil and gun is already pointing at aim direction - keep it there
            targetRot = currentRot;
        }
    }

    private float GetTimedAlpha()
    {
        // Calculate slerp alpha
        bool shouldPullDown = secondsBeforePulldown == 0f;

        float multiplier = shouldPullDown ? PullbackAlphaMultiplier : RecoilAlphaMultiplier;

        float baseAlpha = shouldPullDown ? PullBackAlpha : RecoilSlerpAlpha;

        float timedAlpha = baseAlpha * Time.deltaTime;

        // Apply optional multiplier to accelerate slerp
        if (shouldPullDown)
        {
            timedAlpha *= UsePullbackMultiplier ? multiplier : 1f;
        }
        else
        {
            timedAlpha *= UseRecoilMultiplier ? multiplier : 1f;
        }

        return timedAlpha;
    }
}
