using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AmbientSoundManager : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("El clip de audio que se reproducirá como sonido ambiente.")]
    [SerializeField] private AudioClip ambientClip;
    
    [Range(0f, 1f)]
    [Tooltip("Volumen inicial del sonido ambiente.")]
    [SerializeField] private float volume = 0.5f;

    [Range(0.1f, 3f)]
    [Tooltip("Velocidad de reproducción del sonido.")]
    [SerializeField] private float pitch = 1.0f;

    [Tooltip("¿Debería repetirse en bucle de forma infinita? (Recomendado para ambientes)")]
    [SerializeField] private bool loop = true;

    [Tooltip("¿Debería empezar a reproducirse automáticamente al iniciar la escena?")]
    [SerializeField] private bool playOnAwake = true;

    [Header("Transitions")]
    [Tooltip("¿Debería hacer un efecto de Fade In (aparición gradual) al iniciar?")]
    [SerializeField] private bool fadeInOnStart = false;
    
    [Tooltip("Duración en segundos de la aparición gradual.")]
    [SerializeField] private float fadeInDuration = 2.0f;

    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // Obtener el componente AudioSource requerido
        audioSource = GetComponent<AudioSource>();
        
        // Configurar el AudioSource con los parámetros del inspector
        audioSource.clip = ambientClip;
        audioSource.volume = fadeInOnStart ? 0f : volume;
        audioSource.pitch = pitch;
        audioSource.loop = loop;
        audioSource.playOnAwake = playOnAwake;
        
        // Si es música/ambiente de fondo, usualmente es sonido 2D (no 3D espacial)
        // para que se escuche igual en toda la escena sin importar la posición de la cámara/jugador.
        audioSource.spatialBlend = 0f; 
    }

    private void Start()
    {
        if (playOnAwake && ambientClip != null)
        {
            if (fadeInOnStart)
            {
                PlayWithFadeIn(fadeInDuration);
            }
            else
            {
                audioSource.Play();
            }
        }
    }

    /// <summary>
    /// Reproduce el sonido ambiente inmediatamente.
    /// </summary>
    public void Play()
    {
        if (audioSource == null) return;
        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        audioSource.volume = volume;
        audioSource.Play();
    }

    /// <summary>
    /// Detiene la reproducción del sonido ambiente.
    /// </summary>
    public void Stop()
    {
        if (audioSource == null) return;
        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        audioSource.Stop();
    }

    /// <summary>
    /// Pausa el sonido ambiente.
    /// </summary>
    public void Pause()
    {
        if (audioSource == null) return;
        audioSource.Pause();
    }

    /// <summary>
    /// Reanuda el sonido ambiente.
    /// </summary>
    public void UnPause()
    {
        if (audioSource == null) return;
        audioSource.UnPause();
    }

    /// <summary>
    /// Cambia dinámicamente el volumen del sonido.
    /// </summary>
    /// <param name="newVolume">Nuevo volumen entre 0 y 1</param>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null && fadeCoroutine == null)
        {
            audioSource.volume = volume;
        }
    }

    /// <summary>
    /// Cambia el clip de audio en tiempo de ejecución.
    /// </summary>
    /// <param name="newClip">El nuevo clip de audio.</param>
    /// <param name="playImmediately">¿Debería empezar a reproducirse de inmediato?</param>
    public void ChangeClip(AudioClip newClip, bool playImmediately = true)
    {
        if (audioSource == null) return;
        
        ambientClip = newClip;
        audioSource.clip = newClip;
        
        if (playImmediately && newClip != null)
        {
            audioSource.Play();
        }
    }

    /// <summary>
    /// Inicia la reproducción con una aparición gradual del volumen (Fade In).
    /// </summary>
    /// <param name="duration">Duración de la transición en segundos.</param>
    public void PlayWithFadeIn(float duration)
    {
        if (audioSource == null || ambientClip == null) return;
        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        audioSource.Play();
        fadeCoroutine = StartCoroutine(FadeVolumeRoutine(0f, volume, duration));
    }

    /// <summary>
    /// Detiene la reproducción de forma gradual desvaneciendo el volumen (Fade Out).
    /// </summary>
    /// <param name="duration">Duración de la transición en segundos.</param>
    public void StopWithFadeOut(float duration)
    {
        if (audioSource == null || !audioSource.isPlaying) return;
        
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeVolumeRoutine(audioSource.volume, 0f, duration, true));
    }

    private IEnumerator FadeVolumeRoutine(float startVolume, float targetVolume, float duration, bool stopAtEnd = false)
    {
        float currentTime = 0f;
        audioSource.volume = startVolume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        
        if (stopAtEnd)
        {
            audioSource.Stop();
        }
        
        fadeCoroutine = null;
    }
}
