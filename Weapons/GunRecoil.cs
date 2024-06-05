using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    // Predefined public parameters are tailored for MP5 or other small SMGs
    public Transform RecoilPivot;

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

    private void Start()
    {
        targetRot = RecoilPivot.localRotation.eulerAngles;
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
            case WeaponSpriteController.LookSide.Left:
                // targetRotation = new Vector3(targetRotation.x, targetRotation.y, targetRotation.z - KickPerShot);
                pendingKick -= KickPerShot;
                break;
            case WeaponSpriteController.LookSide.Right:
                // targetRotation = new Vector3(targetRotation.x, targetRotation.y, targetRotation.z + KickPerShot);
                pendingKick += KickPerShot;
                break;
        }
    }

    private void Update()
    {
        // Get rotation data
        var currentRot = RecoilPivot.transform.localRotation.eulerAngles;

        // Recoil upwards if shots fired, else pull weapon back down
        bool shouldPullDown = secondsBeforePulldown == 0f;
        if (shouldPullDown)
        {
            targetRot = new Vector3(currentRot.x, currentRot.y, 0f); // Target rotation is always 0 on z axis when pulling down
        }
        else
        {
            // Add pending kick (degrees) to target rotation
            targetRot = new Vector3(currentRot.x, currentRot.y, currentRot.z + pendingKick);
            // Clamp target rotation based on current look side (Prevents gun from recoiling infinitely and doing 360) 
            switch (WeaponSpriteController.CurrentLookSide)
            {
                case WeaponSpriteController.LookSide.Left:
                    targetRot = new Vector3(targetRot.x, targetRot.y, Mathf.Clamp(targetRot.z, -KickDegreesUpperLimit, 0f));
                    break;
                case WeaponSpriteController.LookSide.Right:
                    targetRot = new Vector3(targetRot.x, targetRot.y, Mathf.Clamp(targetRot.z, 0f, KickDegreesUpperLimit));
                    break;
            }
        }

        // Consume pending recoil kick
        pendingKick = 0f;

        // Reduce cooldown if there is any
        if (secondsBeforePulldown > 0f)
        {
            secondsBeforePulldown = Mathf.Clamp(secondsBeforePulldown - Time.deltaTime, 0f, PullDownDelayLength);
        }

        // Calculate slerp alpha
        float timedAlpha;
        if (shouldPullDown)
        {
            // For pullback slerp
            if (UsePullbackMultiplier)
            {
                timedAlpha = PullBackAlpha * Time.deltaTime * PullbackAlphaMultiplier;
            }
            else
            {
                timedAlpha = PullBackAlpha * Time.deltaTime;
            }
        }
        else
        {
            // For recoil slerp
            if (UseRecoilMultiplier)
            {
                timedAlpha = RecoilSlerpAlpha * Time.deltaTime * RecoilAlphaMultiplier;
            }
            else
            {
                timedAlpha = RecoilSlerpAlpha * Time.deltaTime;
            }
        }

        // Add recoil or pullback by slerping towards target rotation
        RecoilPivot.localEulerAngles = Vector3.Slerp(currentRot, targetRot, timedAlpha);
    }
}
