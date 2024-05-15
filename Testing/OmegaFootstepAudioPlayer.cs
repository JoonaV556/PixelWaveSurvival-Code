using UnityEngine;

public class OmegaFootstepAudioPlayer : FootstepAudioComponent
{
    // This script extends the FootstepAudioComponent script to work with the OmegaCharacterController and KinematicCaharacterMotor

    public PlayerController playerController;

    float minWalkSpeed;
    float maxWalkSpeed;
    float normalizedMaxSpeed; // Max speed scaled to range 0 - max speed
    private float normalizedCurrentSpeed;
    private float speedAlpha;

    private void OnEnable()
    {
        // Start listening to the character controller events
        playerController.OnStartedMovingOnGround += OnMovementStarted;
        playerController.OnStoppedMovingOnGround += OnMovementStopped;
        playerController.OnLandedOnGround += OnLandedOnGround;
        playerController.OnJumped += OnJumped;
    }

    private void OnDisable()
    {
        // Stop listening to the character controller events 
        playerController.OnStartedMovingOnGround -= OnMovementStarted;
        playerController.OnStoppedMovingOnGround -= OnMovementStopped;
        playerController.OnLandedOnGround -= OnLandedOnGround;
        playerController.OnJumped -= OnJumped;
    }

    protected override float MovementSpeedAlpha()
    {
        // Get character controllers min and max walk speed
        // (Can be moved to Start() if these values are not expected to change during runtime)
        minWalkSpeed = playerController.MovementSpeedSettings.SneakSpeed;
        maxWalkSpeed = playerController.MovementSpeedSettings.RunSpeed;
        // Calculate normalized max speed (max speed in range 0-1)
        // (Can be moved to Start() if these values are not expected to change during runtime)
        normalizedMaxSpeed = maxWalkSpeed - minWalkSpeed;
        // Normalize the current speed to the range 0 - normalizedMaxSpeed
        normalizedCurrentSpeed = Mathf.Clamp(playerController.velocityThisFrame - minWalkSpeed, 0, normalizedMaxSpeed);
        // Calculate the current walk speed in range 0 - 1
        speedAlpha = Mathf.Clamp(normalizedCurrentSpeed / normalizedMaxSpeed, 0, 1);
        // Return the current walk speed in range 0 - 1
        return speedAlpha;
    }
}
