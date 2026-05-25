using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Content")]
    public Transform contentParent;

    [Header("Animación")]
    public float delayBetweenCards = 0.22f;

    private readonly List<VillageCardUI> cards = new();

    private void Start()
    {
        BuildPanel();
    }

    public void BuildPanel()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        cards.Clear();
        StartCoroutine(BuildAnimated());
    }

    private IEnumerator BuildAnimated()
    {
        foreach (VillagePanelEntry entry in villages)
        {
            VillageCardUI card = Instantiate(cardPrefab, contentParent);
            card.Setup(entry);
            cards.Add(card);

            yield return new WaitForSeconds(delayBetweenCards);
        }
    }
}