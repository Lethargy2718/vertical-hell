using UnityEngine;

public class SoundComponent : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip clip;

    [Header("Pitch")]
    [SerializeField] private float minPitch = 1f;
    [SerializeField] private float maxPitch = 1f;

    [Header("Volume")]
    [SerializeField] private float minVolume = 1f;
    [SerializeField] private float maxVolume = 1f;

    public void Play(bool stopFirst = false)
    {
        if (stopFirst) audioSource.Stop();

        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.volume = Random.Range(minVolume, maxVolume);
        audioSource.PlayOneShot(clip);
    }

    public void Stop() => audioSource.Stop();
}