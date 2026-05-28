using UnityEngine;
using UnityEngine.UI;

public class GameSpeedButton : MonoBehaviour
{
    [Tooltip("Multiplier applied to Time.timeScale when the button is active.")]
    public float speedMultiplier = 2f;

    private bool isFast = false;
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(ToggleSpeed);
    }

    void ToggleSpeed()
    {
        isFast = !isFast;
        Time.timeScale = isFast ? speedMultiplier : 1f;
        Debug.Log($"[GameSpeed] timeScale = {Time.timeScale}");
    }

    void OnDestroy()
    {
        // Restaurar velocidad normal si se destruye el botón
        Time.timeScale = 1f;
    }
}
