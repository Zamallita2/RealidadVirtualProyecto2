using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance;

    [Header("Música")]
    public AudioSource musicSource;

    [Header("Clip")]
    public AudioClip backgroundMusic;

    [Header("Volumen")]
    [Range(0f, 1f)]
    public float volume = 0.6f;

    private void Awake()
    {
        // Evita duplicados
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SetupAudio();
    }

    private void Start()
    {
        musicSource.mute = false;
        musicSource.volume = volume;
        PlayMusic();
    }
    private void SetupAudio()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = volume;
    }

    public void PlayMusic()
    {
        if (backgroundMusic == null)
            return;

        if (musicSource.isPlaying)
            return;

        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PauseMusic()
    {
        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        musicSource.UnPause();
    }

    public void SetVolume(float newVolume)
    {
        volume = newVolume;
        musicSource.volume = volume;
    }
}