using UnityEngine;
using UnityEngine.UI;

public class UIButtonSound : MonoBehaviour
{
    [Header("Botón")]
    public Button targetButton;

    void Start()
    {
        // Si no asignaste botón manualmente
        if (targetButton == null)
        {
            targetButton =
            GetComponent<Button>();
        }

        // Agrega el sonido
        if (targetButton != null)
        {
            targetButton.onClick.AddListener(
                PlaySound
            );
        }
        else
        {
            Debug.LogWarning(
                "UIButtonSound: No hay Button en " +
                gameObject.name
            );
        }
    }

    void PlaySound()
    {
        if (UISoundManager.Instance != null)
        {
            UISoundManager.Instance
            .PlayUIButtonSound();
        }
    }
}