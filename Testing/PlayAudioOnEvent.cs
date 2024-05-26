using UnityEngine;

public class PlayAudioOnEvent : MonoBehaviour
{
    // Test script used for debugging

    // Currently used for testing weapon firing logic

    AudioSource audioSource;

    private void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();
        Firearm.OnShotFired += PlayAudio;
    }

    private void OnDisable()
    {
        Firearm.OnShotFired -= PlayAudio;
    }

    public void PlayAudio(Firearm firearm)
    {
        audioSource.Play();
    }
}
