using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    private Transform targetCamera;

    void Start()
    {
        GameObject cam = GameObject.FindGameObjectWithTag("FightCamera");

        if (cam != null)
        {
            targetCamera = cam.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró cámara con tag FightCamera owo");
        }
    }

    void LateUpdate()
    {
        if (targetCamera == null)
            return;

        transform.LookAt(transform.position + targetCamera.forward,
                         targetCamera.up);
    }
}