using UnityEngine;
using UnityEngine.UI;

public class FilterButton : MonoBehaviour
{
    ButtonSelectionVisual visual;

    public static FilterButton current;

    void Start()
    {
        visual =
        GetComponent<
        ButtonSelectionVisual>();
    }

    public void Click()
    {
        if (current != null)
        {
            current.visual
            .SetSelected(false);
        }

        current = this;

        visual.SetSelected(true);
    }
}