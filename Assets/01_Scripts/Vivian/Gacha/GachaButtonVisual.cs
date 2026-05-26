using UnityEngine;
using UnityEngine.UI;

public class GachaButtonVisual : MonoBehaviour
{
    [Header("Botón")]
    public Button button;
    public Image buttonImage;

    [Header("Sprites")]
    public Sprite enabledSprite;
    public Sprite disabledSprite;

    public void SetEnabledState(bool enabled)
    {
        if (button != null)
            button.interactable = enabled;

        if (buttonImage != null)
            buttonImage.sprite = enabled ? enabledSprite : disabledSprite;
    }
}