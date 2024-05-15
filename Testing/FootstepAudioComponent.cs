using System.Collections;
using UnityEngine;

/// <summary>
/// Simple audio script for character footstep sounds.
/// </summary>
public class FootstepAudioComponent : MonoBehaviour
{
    // Features:
    // - Simple audio script for character footstep sounds
    // - Useful for 3D spatialized footstep sounds on first person characters
    // - Designed to work with any character controller - Triggering the footstep logic is up to the user
    // - Includes optional ability to adjust footstep interval based on character movement speed
    // - Does not track character movement, tracking character movement is up to the user

    // Works by attaching audio sources to each foot object
    // Then plays the footstep sound alternating between foots at a set interval

    // Usage:
    // - Attach this script to any object (preferably a game character)
    // - Assign the footstep sound effect to the FootstepSFX field
    // - Assign the left and right foot transforms to the LeftFoot and RightFoot fields
    // - Adjust the FootstepInterval and FootstepVolume fields as needed
    // - Adjust the feet transform positions to match the character's feet positions, so audio is spatialized correctly
    // - Optionally override the MovementSpeedAlpha method to adjust the footstep interval based on character movement speed
    // Important:
    // - Call the OnMovementStarted method when the character starts moving and OnMovementStopped when it stops, this tells the script when to play footstep sounds
    // - Call the OnLandedOnGround method when the character lands on the ground, this plays the landing sound

    // Note: It might be useful to crete a reusable prefab for the footstep audio sources, and place it as a child of game characters.
    // This way the audio sources follow the characters, but won't interfere with other character logic.

    #region Properties

    [Tooltip("Volume of the footstep sounds")]
    public float FootstepVolume = 1f;
    [Tooltip("Audio clip to use for the footstep sounds")]
    public AudioClip FootstepSFX;
    [Tooltip("Audio clip played when character lands on ground")]
    public AudioClip LandedOnGroundSFX;
    [Tooltip("Audio clip played when character jumps")]
    public AudioClip JumpSFX;
    [Tooltip("Transform for the left foot")]
    public Transform LeftFoot;
    [Tooltip("Transform for the right foot")]
    public Transform RightFoot;
    [Tooltip("Transform for the center point between feet. This is where the ground landing sound is played at.")]
    public Transform CenterPoint;
    [Tooltip("Maximum time between footsteps")]
    public float MaxFootstepInterval = 0.5f;
    [Tooltip("Minimum time bwteen footsteps")]
    public float MinFootstepInterval = 0.3f;
    public bool EnableDebugLogs = false;

    private AudioSource leftFootAudioSource;
    private AudioSource rightFootAudioSource;
    private AudioSource landingAudioSource;
    private AudioSource jumpAudioSource;
    private Coroutine footstepCoroutine;
    private float lastFootstepTime = -1f;
    private bool isRightFoot = true;
    private float actualFootstepInterval = 0.5f; // Smoothly adjusted footstep interval based on character speed and min and max values
    private bool isPlayingFootstep = false;

    #endregion

    protected virtual void Start()
    {
        // Add footstep audio sources to each foot transform
        leftFootAudioSource = LeftFoot.gameObject.AddComponent<AudioSource>();
        rightFootAudioSource = RightFoot.gameObject.AddComponent<AudioSource>();
        landingAudioSource = CenterPoint.gameObject.AddComponent<AudioSource>();
        jumpAudioSource = CenterPoint.gameObject.AddComponent<AudioSource>();
        // Init the audio sources
        leftFootAudioSource.clip = FootstepSFX;
        rightFootAudioSource.clip = FootstepSFX;
        landingAudioSource.clip = LandedOnGroundSFX;
        jumpAudioSource.clip = JumpSFX;
        leftFootAudioSource.spatialBlend = 1;
        rightFootAudioSource.spatialBlend = 1;
        landingAudioSource.spatialBlend = 1;
        jumpAudioSource.spatialBlend = 1;
        leftFootAudioSource.volume = FootstepVolume;
        rightFootAudioSource.volume = FootstepVolume;
        landingAudioSource.volume = FootstepVolume;
        jumpAudioSource.volume = FootstepVolume;
        leftFootAudioSource.loop = false;
        rightFootAudioSource.loop = false;
        landingAudioSource.loop = false;
        jumpAudioSource.loop = false;
    }

    private void Update()
    {
        // Clamp the movement speed alpha to 0-1 range, just in case this is not done in child classes
        float speedAlpha = Mathf.Clamp(MovementSpeedAlpha(), 0f, 1f);
        // Smoothly adjust the footstep interval based on the character's movement speed
        actualFootstepInterval = Mathf.Lerp(MaxFootstepInterval, MinFootstepInterval, speedAlpha);
    }

    /// <summary>
    /// Returns value between 0 and 1 representing the character's current walk speed. 0 means default walk speed, 1 means maximum walk speed. Override this method to provide custom logic for determining the movement speed alpha.
    /// </summary>
    protected virtual float MovementSpeedAlpha()
    {
        // Default implementation, override this method to provide custom logic for determining the movement speed alpha
        return 0f;
    }

    public void OnMovementStarted()
    {
        if (!isPlayingFootstep)
        {
            isPlayingFootstep = true;
            if (EnableDebugLogs)
            {
                Debug.Log("Started playing footstep SFX");
            }
        }
        // Start the footstep coroutine if it's not already running
        if (footstepCoroutine == null)
        {
            footstepCoroutine = StartCoroutine(PlayFootstepSound());
        }
    }

    public void OnMovementStopped()
    {
        if (isPlayingFootstep)
        {
            isPlayingFootstep = false;
            if (EnableDebugLogs)
            {
                Debug.Log("Stopped playing footstep SFX");
            }
        }
        // Stop the footstep coroutine
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
    }

    public void OnLandedOnGround()
    {
        landingAudioSource.Play();
        if (EnableDebugLogs)
        {
            Debug.Log("Played landing SFX");
        }
    }

    public void OnJumped()
    {
        jumpAudioSource.Play();
        if (EnableDebugLogs)
        {
            Debug.Log("Played jump SFX");
        }
    }

    private IEnumerator PlayFootstepSound()
    {
        while (true)
        {
            // Alternate between left and right foot
            if (isRightFoot)
            {
                rightFootAudioSource.Play();
            }
            else
            {
                leftFootAudioSource.Play();
            }

            if (EnableDebugLogs)
            {
                Debug.Log("Played footstep SFX");
            }

            // Toggle the foot for playloop
            isRightFoot = !isRightFoot;

            // Update the last footstep time
            lastFootstepTime = Time.time;

            // Wait for the footstep interval before playing the next sound
            yield return new WaitForSeconds(actualFootstepInterval);
        }
    }
}
