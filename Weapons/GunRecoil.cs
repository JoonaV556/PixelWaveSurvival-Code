using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    public Transform RecoilPivot;


    public Vector3 pivotRotationEulerAngles = Vector3.zero; // Remove later


    public float KickPerShot = 0.1f; // Recoil kick in degrees
    [Range(0, 1)] public float RecoilSlerpAlpha = 0.98f; // How fast gun kicks

    public bool UseMultiplier = false;
    public float RecoilAlphaMultiplier = 1f; // Gun kick speed multiplier, diminishing returns if too high

    public Vector3 targetRot = Vector3.zero; // Make private later

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
    private void AddKick(Firearm firearm)
    {
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
        targetRot = new Vector3(currentRot.x, currentRot.y, currentRot.z + pendingKick);
        pendingKick = 0f;

        // Calculate slerp alpha
        float timedAlpha;
        if (UseMultiplier)
        {
            timedAlpha = RecoilSlerpAlpha * Time.deltaTime * RecoilAlphaMultiplier;
        }
        else
        {
            timedAlpha = RecoilSlerpAlpha * Time.deltaTime;
        }

        // Add recoil by slerping towards target rotation
        RecoilPivot.localEulerAngles = Vector3.Slerp(currentRot, targetRot, timedAlpha);
    }
}
