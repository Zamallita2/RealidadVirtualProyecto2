using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class VillageCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform targetVillage;

    [Header("Cámara")]
    public Vector3 offset = new Vector3(0f, 2.8f, -2.6f);

    [Header("Zoom base")]
    public float orthographicSize = 1.3f;

    [Header("Zoom por evolución")]
    public bool autoZoomByStage = true;
    public VillageEvolution villageEvolution;
    public float campamentoZoom = 1.25f;
    public float ciudadZoom = 1.65f;
    public float metropolisZoom = 2.15f;

    [Header("Fondo")]
    public Color backgroundColor = new Color(0.03f, 0.02f, 0.04f, 1f);

    private Camera cam;

    private void Awake()
    {
        ConfigureCamera();
    }

    private void OnEnable()
    {
        ConfigureCamera();
    }

    private void LateUpdate()
    {
        ConfigureCamera();
        UpdateCamera();
    }

    public void SetTarget(Transform newTarget)
    {
        targetVillage = newTarget;
        UpdateCamera();
    }

    private void ConfigureCamera()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
            return;

        cam.orthographic = true;

        if (autoZoomByStage && villageEvolution != null)
        {
            switch (villageEvolution.currentStage)
            {
                case VillageEvolutionStage.Campamento:
                    cam.orthographicSize = campamentoZoom;
                    break;

                case VillageEvolutionStage.Ciudad:
                    cam.orthographicSize = ciudadZoom;
                    break;

                case VillageEvolutionStage.Metropolis:
                    cam.orthographicSize = metropolisZoom;
                    break;
            }
        }
        else
        {
            cam.orthographicSize = orthographicSize;
        }

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = backgroundColor;
    }

    private void UpdateCamera()
    {
        if (targetVillage == null)
            return;

        transform.position = targetVillage.position + offset;
        transform.LookAt(targetVillage.position);
    }
}