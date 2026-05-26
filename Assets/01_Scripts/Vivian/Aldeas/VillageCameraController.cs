using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class VillageCameraController : MonoBehaviour
{
    [Header("Aldea que verá la cámara")]
    public Transform targetVillage;

    [Header("Cámara más cerca")]
    public Vector3 offset = new Vector3(0f, 2.2f, -2f);

    [Header("Zoom")]
    [Range(0.5f, 5f)]
    public float orthographicSize = 1.1f;

    [Header("Fondo oscuro")]
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

    private void OnValidate()
    {
        ConfigureCamera();
        UpdateCamera();
    }

    private void LateUpdate()
    {
        UpdateCamera();
    }

    private void ConfigureCamera()
    {
        cam = GetComponent<Camera>();

        if (cam == null) return;

        cam.orthographic = true;
        cam.orthographicSize = orthographicSize;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = backgroundColor;
    }

    private void UpdateCamera()
    {
        if (targetVillage == null) return;

        transform.position = targetVillage.position + offset;
        transform.LookAt(targetVillage.position);
    }
}