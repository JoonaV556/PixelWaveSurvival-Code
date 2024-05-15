using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls player movement.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    /* Implements movement by using AddForce() on player's RigidBody2D in the "MovePlayer"-method.
    Player's max speed is controlled by clamping the velocity of the Rigidbody in the "ClampPlayerVelocity"-method. */

    [SerializeField, Tooltip("Adjust this to change player's acceleration and movement speed")]
    private float MovementForce = 900f;

    [SerializeField, Tooltip("Drag player's rigidbody here")]
    private Rigidbody2D Rigidbody;

    [SerializeField, Tooltip("Use this to limit the player movement velocity")]
    private float MaxVelocity = 1.3f;

    private Vector2 InputVector = Vector2.zero;
    Vector2 currentVelocity = Vector2.zero;
    public bool EnableMovement = true;

    // All movement logic is called here
    private void FixedUpdate()
    {
        if (!EnableMovement)
        {
            return;
        }

        if (InputVector != Vector2.zero)
        {
            // Move player
            MovePlayer();
        }
        // Clamp players velocity to prevent accelerating to too high speeds
        ClampPlayerVelocity();
    }

    // Adjust player's max movement speed
    private void ClampPlayerVelocity()
    {
        // Calculate clamped velocity
        currentVelocity = Rigidbody.velocity;
        float clampedX = Mathf.Clamp(currentVelocity.x, -MaxVelocity, MaxVelocity);
        float clampedY = Mathf.Clamp(currentVelocity.y, -MaxVelocity, MaxVelocity);
        // Clamp velocity
        Rigidbody.velocity = new Vector2(clampedX, clampedY);
    }

    // Move player
    private void MovePlayer()
    {
        // Do movement
        Vector2 MoveVector = InputVector * MovementForce * Time.fixedDeltaTime;
        Rigidbody.AddForce(MoveVector, ForceMode2D.Force);
    }

    // Read movement input
    public void OnMove(InputValue input)
    {
        // Read movement direction from input
        InputVector = input.Get<Vector2>().normalized;
    }
}
