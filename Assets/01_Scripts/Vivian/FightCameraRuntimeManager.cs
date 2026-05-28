using UnityEngine;

public class FightCameraRuntimeManager : MonoBehaviour
{
    [Header("Cámaras")]
    public Camera mainCamera;
    public Camera fightCamera;

    [Header("Managers")]
    public FightManager fightManager;
    public AdventurerManager adventurerManager;

    [Header("Opcional: posición fija de la cámara de pelea")]
    public bool forceFightCameraTransform = false;
    public Vector3 fightCameraPosition = new Vector3(0f, 8f, -8f);
    public Vector3 fightCameraRotation = new Vector3(45f, 0f, 0f);

    [Header("Comportamiento")]
    public bool onlyShowFightCameraDuringFight = true;
    public bool disableMainCameraDuringFight = false;

    [Header("Profundidad")]
    public float mainCameraDepth = 0f;
    public float fightCameraDepth = 10f;

    private bool lastFightState;

    private void Awake()
    {
        FindReferences();
        SetupCameras();
    }

    private void Start()
    {
        FindReferences();
        SetupCameras();
        ApplyCameraState();
    }

    private void LateUpdate()
    {
        FindReferences();

        bool fightActive = IsFightActive();

        if (fightActive != lastFightState)
        {
            lastFightState = fightActive;
            ApplyCameraState();
        }

        if (fightActive || !onlyShowFightCameraDuringFight)
        {
            RefreshFightCamera();
        }
    }

    private void FindReferences()
    {
        if (fightManager == null)
            fightManager = FindFirstObjectByType<FightManager>();

        if (adventurerManager == null)
            adventurerManager = FindFirstObjectByType<AdventurerManager>();

        if (mainCamera == null && Camera.main != null)
            mainCamera = Camera.main;

        if (fightCamera == null)
        {
            GameObject camObj = GameObject.FindGameObjectWithTag("FightCamera");

            if (camObj != null)
                fightCamera = camObj.GetComponent<Camera>();
        }
    }

    private void SetupCameras()
    {
        if (mainCamera != null)
        {
            mainCamera.depth = mainCameraDepth;
        }

        if (fightCamera != null)
        {
            fightCamera.depth = fightCameraDepth;
            fightCamera.enabled = !onlyShowFightCameraDuringFight;
            fightCamera.gameObject.SetActive(true);
            fightCamera.targetDisplay = 0;
        }
    }

    private bool IsFightActive()
    {
        if (fightManager != null && fightManager.IsFightActive)
            return true;

        if (adventurerManager != null && adventurerManager.IsFightRunning)
            return true;

        return false;
    }

    private void ApplyCameraState()
    {
        bool fightActive = IsFightActive();

        if (fightCamera != null)
        {
            fightCamera.gameObject.SetActive(true);
            fightCamera.enabled = fightActive || !onlyShowFightCameraDuringFight;
            fightCamera.depth = fightCameraDepth;
        }

        if (mainCamera != null)
        {
            mainCamera.depth = mainCameraDepth;

            if (disableMainCameraDuringFight)
                mainCamera.enabled = !fightActive;
            else
                mainCamera.enabled = true;
        }

        RefreshFightCamera();
    }

    private void RefreshFightCamera()
    {
        if (fightCamera == null)
            return;

        fightCamera.gameObject.SetActive(true);
        fightCamera.enabled = true;
        fightCamera.depth = fightCameraDepth;

        if (forceFightCameraTransform)
        {
            fightCamera.transform.position = fightCameraPosition;
            fightCamera.transform.rotation = Quaternion.Euler(fightCameraRotation);
        }

        fightCamera.ResetWorldToCameraMatrix();
        fightCamera.ResetProjectionMatrix();
    }
}