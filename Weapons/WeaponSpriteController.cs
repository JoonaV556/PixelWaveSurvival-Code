using System.Collections;
using System.Collections.Generic;
using JoonaUtils;
using UnityEngine;

public class WeaponSpriteController : MonoBehaviour
{
    // Points weapon towards mouse position
    // Flips weapon sprite when player is looking left and right, so weapon orientation looks always right

    public Vector3 WeaponRotation;

    public Transform WeaponBodyPivotTransform;

    public SpriteRenderer WeaponSpriteRenderer;

    private void Update()
    {
        var pivotWorldPosition = WeaponBodyPivotTransform.position;

        // Get mouse position
        var mousePos = PlayerInput.LookPositionInput;

        // Get mouse position in world space
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // Get vector pointing towards mouse world pos
        var directionToMouse = Convenience.Direction2D(pivotWorldPosition, mouseWorldPos);

        // Point gun towards mouse position
        WeaponBodyPivotTransform.right = directionToMouse.normalized;

        // Flip gun prite when player is looking left and right
        // If weapon pivot z rotation x < 90 && x > 270, flip the sprite on x axis
        WeaponRotation = WeaponBodyPivotTransform.eulerAngles;

        if (WeaponRotation.z > 90f && WeaponRotation.z < 270f)
        {
            WeaponSpriteRenderer.flipY = true;
            WeaponSpriteRenderer.flipX = true;
        }
        else
        {
            WeaponSpriteRenderer.flipY = false;
            WeaponSpriteRenderer.flipX = true;
        }
    }
}
