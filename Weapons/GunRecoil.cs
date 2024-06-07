using System.Linq.Expressions;
using UnityEngine;

public class GunRecoil : MonoBehaviour
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
        // Simulate kick by offsetting target rotation by degrees
        switch (WeaponSpriteController.CurrentLookSide)
        {
            case LookSide.Left:
                // targetRotation = new Vector3(targetRotation.x, targetRotation.y, targetRotation.z - KickPerShot);
                pendingKick -= KickPerShot;
                break;
            case LookSide.Right:
                // targetRotation = new Vector3(targetRotation.x, targetRotation.y, targetRotation.z + KickPerShot);
                pendingKick += KickPerShot;
                break;
        }
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
            float newZRot = (WeaponSpriteController.CurrentLookSide == LookSide.Left) ? 360 - currentRot.z : 0 + (360 - currentRot.z);
            RecoilPivot.localEulerAngles = new Vector3(currentRot.x, currentRot.y, newZRot);
            currentRot = RecoilPivot.transform.localRotation.eulerAngles; // Update current rotation
            return;
        }

        // Determine target rotation to slerp to
        UpdateTargetRotation();

        // Reset pending recoil kick
        pendingKick = 0f;

        // Reduce cooldown if there is any
        if (secondsBeforePulldown > 0f)
        {
            secondsBeforePulldown = Mathf.Clamp(secondsBeforePulldown - Time.deltaTime, 0f, PullDownDelayLength);
        }

        float timedAlpha = GetTimedAlpha();

        // Apply rotation using slerp
        RecoilPivot.localEulerAngles = Vector3.Slerp(currentRot, targetRot, timedAlpha);
    }

    private void UpdateTargetRotation()
    {
        // Determine target rotation
        if (secondsBeforePulldown == 0f)
        {
            // Recoil ended - Rotate gun back to aim direction
            targetRot = EnablePulldown ? new Vector3(currentRot.x, currentRot.y, 0f) : currentRot;
        }
        else if (pendingKick > 0f)
        {
            // Recoil is still pending - Rotate gun upwards based on look direction
            var currentLookside = WeaponSpriteController.CurrentLookSide;
            targetRot = new Vector3(currentRot.x, currentRot.y, currentRot.z);
            print("Target rot before recoil: " + targetRot.z);

            // Change kick direction based on look side
            targetRot.z = (currentLookside == LookSide.Left) ? 360 - pendingKick : targetRot.z + pendingKick;

            // Clamp rotation to prevent gun from rotating too much
            float zClampMin = (currentLookside == LookSide.Left) ? 360 - KickDegreesUpperLimit : 0f;
            float zClampMax = (currentLookside == LookSide.Left) ? 360f : KickDegreesUpperLimit;
            targetRot.z = Mathf.Clamp(targetRot.z, zClampMin, zClampMax);
        }
        else
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
