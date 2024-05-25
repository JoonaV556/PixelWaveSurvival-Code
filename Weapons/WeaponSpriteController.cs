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

    public bool PointTowardsCursorPosition = true;
    public bool FlipSpriteOnHorizontalAxis = true;
    public bool FlipSpriteYOffsetOnHorizontalAxis = true;

    bool weaponsSpriteHasYOffset = false;

    // TODO
    // Get sprite object y offset at start
    // If sprite object has any offset on y axis, flip the offset to keep weapon on same elevation on left and right

    private void Start()
    {
        weaponsSpriteHasYOffset = WeaponSpriteRenderer.transform.position.y != 0f;
    }

    private void Update()
    {
        var pivotWorldPosition = WeaponBodyPivotTransform.position;

        // Get mouse position
        var mousePos = PlayerInput.LookPositionInput;

        // Get mouse position in world space
        var mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // Get vector pointing towards mouse world pos
        var directionToMouse = Convenience.Direction2D(pivotWorldPosition, mouseWorldPos);

        // Point weapon towards mouse position
        if (PointTowardsCursorPosition)
        {
            PointWeaponTowardsCursor(directionToMouse);
        }

        // Flip gun prite when player is looking left and right
        // If sprite object has any offset on y axis, flip the offset to keep weapon on same elevation on left and right
        // If weapon pivot z rotation x < 90 && x > 270, flip the sprite on x axis
        WeaponRotation = WeaponBodyPivotTransform.eulerAngles;

        bool shouldFlipStuff = WeaponRotation.z > 90f && WeaponRotation.z < 270f;

        // Flip sprite on horizontal axis
        if (FlipSpriteOnHorizontalAxis)
        {
            FlipWeaponSpriteOnHorizontal(shouldFlipStuff);
        }

        // Flip sprite y offset on horizontal axis
        if (FlipSpriteYOffsetOnHorizontalAxis)
        {
            FlipSpritePositionYOffset(shouldFlipStuff);
        }
    }

    // Flips the y offset of the weapon sprite to keep sprite on same elevation on left and right side
    // WIP - not working as intended
    private void FlipSpritePositionYOffset(bool shouldFlipSpriteYOffset)
    {
        if (!weaponsSpriteHasYOffset)
        {
            return;
        }

        if (shouldFlipSpriteYOffset)
        {
            var flippedYOffset = -1 * WeaponSpriteRenderer.transform.position.y;
            WeaponSpriteRenderer.transform.position = new Vector3(WeaponSpriteRenderer.transform.position.x, flippedYOffset, WeaponSpriteRenderer.transform.position.z);
        }
        else
        {
            var flippedYOffset = -1 * WeaponSpriteRenderer.transform.position.y;
            WeaponSpriteRenderer.transform.position = new Vector3(WeaponSpriteRenderer.transform.position.x, flippedYOffset, WeaponSpriteRenderer.transform.position.z);
        }
    }

    // Flips weapon sprite when player is looking left and right to keep gun barrel pointing outwards from player
    private void FlipWeaponSpriteOnHorizontal(bool shouldFlipSprite)
    {
        if (shouldFlipSprite)
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

    // Assumes weapons pointing direction is WeaponBodyPivots red (x, right) axis
    // Weapon sprite barrel should be pointing towards red axis
    private void PointWeaponTowardsCursor(Vector2 directionToMouse)
    {
        // Point gun towards mouse position
        WeaponBodyPivotTransform.right = directionToMouse.normalized;
    }
}
