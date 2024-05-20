using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player movement.
/// </summary>
public class CharacterMovement : MonoBehaviour
{
    /* Implements movement by using AddForce() on player's RigidBody2D in the "MovePlayer"-method.
    Player's max speed is controlled by clamping the velocity of the Rigidbody in the "ClampPlayerVelocity"-method. */

    [SerializeField, Tooltip("Adjust this to change player's acceleration and movement speed")]
    private float MovementForce = 600000f; // Default value adjusted for Rigidbody mass of 70kg

    [SerializeField, Tooltip("Drag player's rigidbody here")]
    private Rigidbody2D Rigidbody;

    [SerializeField, Tooltip("Use this to limit the player movement velocity")]
    private float MaxVelocity = 6f; // Default value adjusted for Rigidbody mass of 70kg

    protected Vector2 MoveInput = Vector2.zero;
    Vector2 currentVelocity = Vector2.zero;
    public bool EnableMovement = true;

    public bool ClampVelocity = true;

    // All movement logic is called here
    private void FixedUpdate()
    {
        if (!EnableMovement)
        {
            return;
        }

        if (MoveInput != Vector2.zero)
        {
            // Move player
            MovePlayer();
        }
        // Clamp players velocity to prevent accelerating to too high speeds
        if (ClampVelocity) ClampMoveVelocity();
    }

    protected virtual void FetchInput() { }

    private void Update()
    {
        // Fetch input 
        FetchInput();
    }

    // Adjust player's max movement speed
    private void ClampMoveVelocity()
    {
        // Calculate clamped velocity
        currentVelocity = Rigidbody.velocity;
        float clampedX = Mathf.Clamp(currentVelocity.x, -MaxVelocity, MaxVelocity);
        float clampedY = Mathf.Clamp(currentVelocity.y, -MaxVelocity, MaxVelocity);
        // Clamp velocity
        Rigidbody.velocity = new Vector2(clampedX, clampedY);

        var vec = Vector2.zero;
    }

    // Move player
    private void MovePlayer()
    {
        // Do movement
        Vector2 MoveVector = MoveInput * MovementForce * Time.fixedDeltaTime;
        Rigidbody.AddForce(MoveVector, ForceMode2D.Force);
    }
}
