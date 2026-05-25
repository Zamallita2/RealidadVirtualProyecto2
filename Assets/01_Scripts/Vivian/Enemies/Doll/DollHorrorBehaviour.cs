using UnityEngine;

[DisallowMultipleComponent]
public class DollHorrorBehaviour : MonoBehaviour
{
    [Header("Partes visuales")]
    public Transform head;
    public Transform body;
    public Renderer[] renderersToPulse;

    [Header("Mirada incómoda opcional")]
    public bool lookAtMainCamera = true;
    public float headLookSpeed = 1.4f;
    public float maxHeadAngle = 28f;

    [Header("Movimiento inquietante")]
    public float idleSwayAmount = 1.5f;
    public float idleSwaySpeed = 0.7f;
    public float twitchIntervalMin = 2.5f;
    public float twitchIntervalMax = 5.5f;
    public float twitchAngle = 7f;
    public float twitchDuration = 0.08f;

    [Header("Iluminación creepy")]
    public Light creepyLight;
    public float minLightIntensity = 0.2f;
    public float maxLightIntensity = 1.1f;
    public float flickerSpeed = 7f;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip whisperLoop;
    public AudioClip porcelainStep;
    public AudioClip attackSound;
    public AudioClip deathSound;
    public float stepInterval = 0.8f;

    private Animator animator;
    private UnitStats stats;
    private Quaternion headStartRotation;
    private Quaternion bodyStartRotation;
    private float nextTwitchTime;
    private float twitchTimer;
    private float stepTimer;
    private bool deathPlayed;

    void Awake()
    {
        animator = GetComponent<Animator>();
        if(animator == null)
            animator = GetComponentInChildren<Animator>();

        stats = GetComponent<UnitStats>();

        if(audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if(audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1f;
        audioSource.loop = false;
        audioSource.playOnAwake = false;

        if(head != null)
            headStartRotation = head.localRotation;

        if(body != null)
            bodyStartRotation = body.localRotation;

        ProgramarSiguienteTwitch();
    }

    void Start()
    {
        if(whisperLoop != null)
        {
            audioSource.clip = whisperLoop;
            audioSource.loop = true;
            audioSource.volume = 0.45f;
            audioSource.Play();
        }
    }

    void Update()
    {
        if(stats != null && !stats.isAlive)
        {
            ReproducirMuerteUnaVez();
            return;
        }

        AnimarCuerpoLento();
        AnimarCabezaIncomoda();
        ParpadeoDeLuz();
        ReproducirPasosDePorcelana();
    }

    void AnimarCuerpoLento()
    {
        if(body == null)
            return;

        float sway = Mathf.Sin(Time.time * idleSwaySpeed) * idleSwayAmount;
        body.localRotation = bodyStartRotation * Quaternion.Euler(0f, sway, 0f);
    }

    void AnimarCabezaIncomoda()
    {
        if(head == null)
            return;

        Quaternion targetRotation = headStartRotation;

        if(lookAtMainCamera && Camera.main != null)
        {
            Vector3 dir = Camera.main.transform.position - head.position;
            dir.y = Mathf.Clamp(dir.y, -0.4f, 0.4f);

            if(dir.sqrMagnitude > 0.01f)
            {
                Quaternion worldLook = Quaternion.LookRotation(dir.normalized);
                Quaternion localLook = Quaternion.Inverse(transform.rotation) * worldLook;
                Vector3 euler = localLook.eulerAngles;

                euler.x = NormalizarAngulo(euler.x);
                euler.y = NormalizarAngulo(euler.y);
                euler.z = 0f;

                euler.x = Mathf.Clamp(euler.x, -maxHeadAngle, maxHeadAngle);
                euler.y = Mathf.Clamp(euler.y, -maxHeadAngle, maxHeadAngle);

                targetRotation = headStartRotation * Quaternion.Euler(euler.x, euler.y, 0f);
            }
        }

        if(Time.time >= nextTwitchTime)
        {
            twitchTimer = twitchDuration;
            ProgramarSiguienteTwitch();
        }

        if(twitchTimer > 0f)
        {
            twitchTimer -= Time.deltaTime;
            targetRotation *= Quaternion.Euler(
                Random.Range(-twitchAngle, twitchAngle),
                Random.Range(-twitchAngle, twitchAngle),
                Random.Range(-twitchAngle, twitchAngle)
            );
        }

        head.localRotation = Quaternion.Slerp(
            head.localRotation,
            targetRotation,
            headLookSpeed * Time.deltaTime
        );
    }

    void ParpadeoDeLuz()
    {
        if(creepyLight == null)
            return;

        float ruido = Mathf.PerlinNoise(Time.time * flickerSpeed, 0.31f);
        creepyLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, ruido);
    }

    void ReproducirPasosDePorcelana()
    {
        if(animator == null || porcelainStep == null || audioSource == null)
            return;

        bool isWalking = animator.GetBool("IsWalking");

        if(!isWalking)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer += Time.deltaTime;

        if(stepTimer >= stepInterval)
        {
            stepTimer = 0f;
            audioSource.PlayOneShot(porcelainStep, 0.7f);
        }
    }

    void ReproducirMuerteUnaVez()
    {
        if(deathPlayed)
            return;

        deathPlayed = true;

        if(audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();

            if(deathSound != null)
                audioSource.PlayOneShot(deathSound, 1f);
        }
    }

    void ProgramarSiguienteTwitch()
    {
        nextTwitchTime = Time.time + Random.Range(twitchIntervalMin, twitchIntervalMax);
    }

    float NormalizarAngulo(float angle)
    {
        if(angle > 180f)
            angle -= 360f;

        return angle;
    }

    public void PlayAttackSound()
    {
        if(audioSource != null && attackSound != null)
            audioSource.PlayOneShot(attackSound, 1f);
    }
}
