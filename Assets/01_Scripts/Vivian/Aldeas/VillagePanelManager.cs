using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class VillagePanelEntry
{
    public VillageData villageData;
    public RenderTexture renderTexture;

    [Header("Visual")]
    public Sprite backgroundSprite;
    public Sprite frameSprite;

    [Header("Personajes")]
    public List<Sprite> portraits = new List<Sprite>();
}

public class VillagePanelManager : MonoBehaviour
{
    [Header("Aldeas")]
    public List<VillagePanelEntry> villages = new();

    [Header("Prefab")]
    public VillageCardUI cardPrefab;

    [Header("Content del Scroll")]
    public RectTransform contentParent;

    private readonly List<VillageCardUI> cards = new();
    private bool alreadyBuilt;

    private void Start()
    {
        BuildPanel();
    }

    public void BuildPanel()
    {
        if (alreadyBuilt)
            return;

        alreadyBuilt = true;

        ClearPanel();

        for (int i = 0; i < villages.Count; i++)
        {
            VillageCardUI card = Instantiate(cardPrefab, contentParent);

            RectTransform rt = card.GetComponent<RectTransform>();
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
            rt.localPosition = Vector3.zero;

            card.Setup(villages[i]);
            cards.Add(card);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
        Canvas.ForceUpdateCanvases();
    }

    private void ClearPanel()
    {
        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        cards.Clear();
    }
}