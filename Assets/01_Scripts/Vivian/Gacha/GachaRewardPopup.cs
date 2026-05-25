using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GachaRewardPopup : MonoBehaviour
{
    [Header("Panel visual")]
    public GameObject panelObject;
    public CanvasGroup canvasGroup;
    public Image panelImage;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip newCardSound;
    public AudioClip duplicateSound;

    [Header("Animación")]
    public float appearTime = 0.45f;
    public float startScale = 0.78f;
    public float overshootScale = 1.05f;
    public float finalScale = 1f;

    private RectTransform panelRect;
    private bool canClose;

    public bool IsShowing { get; private set; }

    private void Awake()
    {
        if (panelObject != null)
        {
            panelRect = panelObject.GetComponent<RectTransform>();
            panelObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsShowing || !canClose)
            return;

        if (Input.GetMouseButtonUp(0) || TouchReleased())
            Hide();
    }

    public void Show(EnemyGachaData data, bool isNew)
    {
        if (data == null) return;

        Sprite selectedSprite = isNew
            ? data.newRewardPanelSprite
            : data.duplicateRewardPanelSprite;

        if (selectedSprite == null)
        {
            Debug.LogError("Falta panel en GD: " + data.enemyName);
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShowRoutine(selectedSprite, isNew));
    }

    private IEnumerator ShowRoutine(Sprite spriteToShow, bool isNew)
    {
        IsShowing = true;
        canClose = false;

        panelObject.SetActive(true);
        panelObject.transform.SetAsLastSibling();

        panelImage.sprite = spriteToShow;
        panelImage.color = Color.white;
        panelImage.enabled = true;
        panelImage.preserveAspect = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panelRect.localScale = Vector3.one * startScale;
        }

        if (audioSource != null)
        {
            AudioClip clip = isNew ? newCardSound : duplicateSound;
            if (clip != null)
                audioSource.PlayOneShot(clip);
        }

        float t = 0f;

        while (t < appearTime)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / appearTime);

            if (canvasGroup != null)
                canvasGroup.alpha = p;

            if (panelRect != null)
            {
                float scale = p < 0.75f
                    ? Mathf.Lerp(startScale, overshootScale, p / 0.75f)
                    : Mathf.Lerp(overshootScale, finalScale, (p - 0.75f) / 0.25f);

                panelRect.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (panelRect != null)
            panelRect.localScale = Vector3.one * finalScale;

        yield return null;
        canClose = true;
    }

    public void Hide()
    {
        if (!IsShowing) return;

        StopAllCoroutines();

        if (panelObject != null)
            panelObject.SetActive(false);

        IsShowing = false;
        canClose = false;
    }

    public IEnumerator WaitUntilClosed()
    {
        while (IsShowing)
            yield return null;
    }

    private bool TouchReleased()
    {
        if (Input.touchCount == 0)
            return false;

        for (int i = 0; i < Input.touchCount; i++)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Ended)
                return true;
        }

        return false;
    }
}