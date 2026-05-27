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

    private void Awake()
    {
        Instance = this;
    }

    public void PlayRoomLightSound()
    {
        if (roomLightSound != null)
        {
            audioSource.PlayOneShot(roomLightSound);
        }
    }

    public void PlayRoomButtonSound()
    {
        if (roomButtonSound != null)
        {
            audioSource.PlayOneShot(roomButtonSound);
        }
    }

    public void PlayUIButtonSound()
    {
        if (uiButtonSound != null)
        {
            audioSource.PlayOneShot(uiButtonSound);
        }
    }
}