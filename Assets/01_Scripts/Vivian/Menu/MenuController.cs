using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Escena")]
    public string sceneToLoad = "aldea,sala,grid,gacha Robert";

    [Header("UI")]
    public Button playButton;
    public RectTransform titleImage;
    public RectTransform playButtonTransform;

    [Header("Música de fondo")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;
    public float musicVolume = 0.5f;
    public bool loopMusic = true;

    [Header("Sonidos UI")]
    public AudioSource sfxSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public float hoverVolume = 0.7f;
    public float clickVolume = 0.8f;

    [Header("Animación del título")]
    public float titleMoveAmount = 8f;
    public float titleMoveSpeed = 1.2f;
    public float titleScaleAmount = 0.025f;

    [Header("Animación del botón")]
    public float buttonMoveAmount = 5f;
    public float buttonMoveSpeed = 1.8f;
    public float buttonScaleAmount = 0.035f;
    public float buttonHoverScale = 1.08f;

    [Header("Carga")]
    public float extraDelayBeforeLoad = 0.15f;

    private Vector2 titleStartPos;
    private Vector2 buttonStartPos;

    private Vector3 titleStartScale;
    private Vector3 buttonStartScale;

    private bool isHoveringButton = false;
    private bool isLoading = false;

    private void Awake()
    {
        SetupAudioSources();
    }

    private void Start()
    {
        SaveInitialUIValues();
        PlayMusic();
        SetupButton();
    }

    private void Update()
    {
        AnimateTitle();
        AnimateButton();
        CheckButtonHover();
    }

    private void SetupAudioSources()
    {
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = loopMusic;
        musicSource.volume = musicVolume;

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = 1f;
    }

    private void SaveInitialUIValues()
    {
        if (titleImage != null)
        {
            titleStartPos = titleImage.anchoredPosition;
            titleStartScale = titleImage.localScale;
        }

        if (playButtonTransform != null)
        {
            buttonStartPos = playButtonTransform.anchoredPosition;
            buttonStartScale = playButtonTransform.localScale;
        }
        else if (playButton != null)
        {
            playButtonTransform = playButton.GetComponent<RectTransform>();
            buttonStartPos = playButtonTransform.anchoredPosition;
            buttonStartScale = playButtonTransform.localScale;
        }
    }

    private void PlayMusic()
    {
        if (backgroundMusic == null)
            return;

        musicSource.clip = backgroundMusic;
        musicSource.volume = musicVolume;
        musicSource.loop = loopMusic;
        musicSource.Play();
    }

    private void SetupButton()
    {
        if (playButton == null)
        {
            Debug.LogWarning("No asignaste el botón Jugar en el Inspector.");
            return;
        }

        playButton.onClick.RemoveAllListeners();
        playButton.onClick.AddListener(PlayGame);
    }

    private void CheckButtonHover()
    {
        if (playButton == null)
            return;

        RectTransform buttonRect = playButton.GetComponent<RectTransform>();

        bool mouseIsOver = RectTransformUtility.RectangleContainsScreenPoint(
            buttonRect,
            Input.mousePosition,
            null
        );

        if (mouseIsOver && !isHoveringButton)
        {
            isHoveringButton = true;
            PlayHoverSound();
        }
        else if (!mouseIsOver && isHoveringButton)
        {
            isHoveringButton = false;
        }
    }

    private void AnimateTitle()
    {
        if (titleImage == null)
            return;

        float move = Mathf.Sin(Time.time * titleMoveSpeed) * titleMoveAmount;
        float scale = 1f + Mathf.Sin(Time.time * titleMoveSpeed) * titleScaleAmount;

        titleImage.anchoredPosition = titleStartPos + new Vector2(0f, move);
        titleImage.localScale = titleStartScale * scale;
    }

    private void AnimateButton()
    {
        if (playButtonTransform == null)
            return;

        float move = Mathf.Sin(Time.time * buttonMoveSpeed) * buttonMoveAmount;
        float idleScale = 1f + Mathf.Sin(Time.time * buttonMoveSpeed) * buttonScaleAmount;

        Vector3 targetScale = buttonStartScale * idleScale;

        if (isHoveringButton)
            targetScale = buttonStartScale * buttonHoverScale;

        playButtonTransform.anchoredPosition = buttonStartPos + new Vector2(0f, move);
        playButtonTransform.localScale = Vector3.Lerp(
            playButtonTransform.localScale,
            targetScale,
            Time.deltaTime * 8f
        );
    }

    private void PlayHoverSound()
    {
        if (sfxSource != null && hoverSound != null)
            sfxSource.PlayOneShot(hoverSound, hoverVolume);
    }

    private void PlayClickSound()
    {
        if (sfxSource != null && clickSound != null)
            sfxSource.PlayOneShot(clickSound, clickVolume);
    }

    public void PlayGame()
    {
        if (isLoading)
            return;

        StartCoroutine(PlayClickAndLoadScene());
    }

    private IEnumerator PlayClickAndLoadScene()
    {
        isLoading = true;

        PlayClickSound();

        float waitTime = extraDelayBeforeLoad;

        if (clickSound != null)
            waitTime += clickSound.length;

        yield return new WaitForSeconds(waitTime);

        SceneManager.LoadScene(sceneToLoad);
    }
}