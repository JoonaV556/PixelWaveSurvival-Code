using JoonaUtils;
using UnityEngine;

public enum LookSide
{
    Left,
    Right
}

public class WeaponSpriteController : MonoBehaviour
{


    // Points weapon towards mouse position
    // Flips weapon sprite when player is looking left and right, so weapon orientation looks always right

    public Vector3 WeaponRotation; // Debugging - remove later

    public Transform WeaponBodyPivotTransform;

    public SpriteRenderer WeaponSpriteRenderer;

    public bool PointTowardsCursorPosition = true;
    public bool FlipSpriteOnHorizontalAxis = true;
    public bool FlipSpriteYOffsetOnHorizontalAxis = true;

    public static LookSide CurrentLookSide = LookSide.Right;

    // TODO
    // Get sprite object y offset at start
    // If sprite object has any offset on y axis, flip the offset to keep weapon on same elevation on left and right

    private void Start()
    {
        weaponsSpriteHasYOffset = WeaponSpriteRenderer.transform.localPosition.y != 0f;
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

        // Update look dir for other systems
        if (shouldFlipStuff)
        {
            CurrentLookSide = LookSide.Left;
        }
        else
        {
            CurrentLookSide = LookSide.Right;
        }

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

    // Flips the y offset of the weapon sprite to keep sprite on same elevation on left and right side (if using sprite flipping)
    bool offsetFlipped = false;
    bool weaponsSpriteHasYOffset = false;
    private void FlipSpritePositionYOffset(bool shouldFlipSpriteYOffset)
    {
        // Do nothing if weapon sprite has no y offset
        if (!weaponsSpriteHasYOffset)
        {
            return;
        }

        if (shouldFlipSpriteYOffset)
        {
            // Prevent flipping offset if its already flipped
            if (offsetFlipped)
            {
                return;
            }
            // Flip
            var flippedYOffset = -1 * WeaponSpriteRenderer.transform.localPosition.y;

            WeaponSpriteRenderer.transform.localPosition = new Vector3(
                WeaponSpriteRenderer.transform.localPosition.x,
                flippedYOffset,
                WeaponSpriteRenderer.transform.localPosition.z
                );
            offsetFlipped = true;
            //Debug.Log("Flipped offset");
        }
        else
        {
            // Prevent flipping offset if its already flipped
            if (!offsetFlipped)
            {
                return;
            }
            // Flip
            var flippedYOffset = -1 * WeaponSpriteRenderer.transform.localPosition.y;

            WeaponSpriteRenderer.transform.localPosition = new Vector3(
                WeaponSpriteRenderer.transform.localPosition.x,
                flippedYOffset,
                WeaponSpriteRenderer.transform.localPosition.z
                );
            offsetFlipped = false;
            //Debug.Log("Unflipped offset");
        }
    }

    // Flips weapon sprite when player is looking left and right to keep gun barrel pointing outwards from player
    bool weaponSpriteFlipped = false;
    private void FlipWeaponSpriteOnHorizontal(bool shouldFlipSprite)
    {
        if (shouldFlipSprite)
        {
            if (!weaponSpriteFlipped)
            {
                WeaponSpriteRenderer.flipY = true;
                WeaponSpriteRenderer.flipX = true;
                weaponSpriteFlipped = true;
            }
        }
        else
        {
            if (weaponSpriteFlipped)
            {
                WeaponSpriteRenderer.flipY = false;
                WeaponSpriteRenderer.flipX = true;
                weaponSpriteFlipped = false;
            }
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
