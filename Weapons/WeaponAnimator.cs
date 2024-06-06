using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles weapon animation playback
/// </summary>
public class WeaponAnimator : MonoBehaviour
{
    // Each weapon has two separate animation clips for left and right side recoil so gun goes always upwards

    public Animator Animator;

    private void OnEnable()
    {
        Firearm.OnShotFired += PlayRecoilAnimation;
    }

    private void OnDisable()
    {
        Firearm.OnShotFired -= PlayRecoilAnimation;
    }

    private void PlayRecoilAnimation(Firearm firearm)
    {
        // print("Playing recoil animation");

        switch (WeaponSpriteController.CurrentLookSide)
        {
            case LookSide.Left:
                Animator.Play("RecoilLeft");
                break;
            case LookSide.Right:
                Animator.Play("RecoilRight");
                break;
        }
    }

}
