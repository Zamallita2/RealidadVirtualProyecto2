using UnityEngine;

public class UISoundManager : MonoBehaviour
{
    public static UISoundManager Instance;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Sonidos")]
    public AudioClip roomLightSound;

    public AudioClip roomButtonSound;

    public AudioClip uiButtonSound;

    public AudioClip hoverSound;

    void Awake()
    {
        Instance = this;
    }

    // SONIDO 1
    public void PlayRoomLightSound()
    {
        if (roomLightSound != null)
        {
            audioSource.PlayOneShot(
                roomLightSound
            );
        }
    }

    // SONIDO 2
    public void PlayRoomButtonSound()
    {
        if (roomButtonSound != null)
        {
            audioSource.PlayOneShot(
                roomButtonSound
            );
        }
    }

    // SONIDO 3
    public void PlayUIButtonSound()
    {
        if (uiButtonSound != null)
        {
            audioSource.PlayOneShot(
                uiButtonSound
            );
        }
    }

    // HOVER
    public void PlayHoverSound()
    {
        if (hoverSound != null)
        {
            audioSource.PlayOneShot(
                hoverSound
            );
        }
    }
}