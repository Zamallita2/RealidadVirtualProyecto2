using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DetectorClicks : MonoBehaviour
{
    public Camera modelCamera;
    public RawImage rawImage;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 localPoint;

            RectTransform rectTransform =
                rawImage.rectTransform;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                Input.mousePosition,
                null,
                out localPoint))
            {
                Rect rect = rectTransform.rect;

                float x = (localPoint.x - rect.x) / rect.width;
                float y = (localPoint.y - rect.y) / rect.height;

                Ray ray = modelCamera.ViewportPointToRay(
                    new Vector3(x, y, 0)
                );

                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    ClickHabitacion habitacion =
                        hit.collider.GetComponent<ClickHabitacion>();

                    if (habitacion != null)
                    {
                        habitacion.Click();
                    }
                }
            }
        }
    }
}