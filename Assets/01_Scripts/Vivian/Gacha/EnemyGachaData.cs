using UnityEngine;

public enum GachaRarity
{
    Comun,
    Raro,
    Epico,
    Legendario,
    Mitico
}

[CreateAssetMenu(fileName = "EnemyGachaData", menuName = "Gacha/Enemy Gacha Data")]
public class EnemyGachaData : ScriptableObject
{
    [Header("ID único")]
    public string enemyId;

    [Header("Datos")]
    public string enemyName;
    public GachaRarity rarity;

    [TextArea(2, 5)]
    public string description;

    [Header("Visual Gacha")]
    public Sprite cardFrontSprite;
    public Sprite cardBackSprite;
    public Sprite newRewardPanelSprite;
    public Sprite duplicateRewardPanelSprite;

    [Header("Visual Tienda")]
    public Sprite shopUnlockedCardSprite;
    public Sprite shopLockedCardSprite;
    public Sprite shopBigUnlockedCardSprite;
    public Sprite shopBigLockedCardSprite;

    [Header("Prefab")]
    public GameObject enemyPrefab;

    [Header("Escala habitación")]
    public Vector3 roomScale =
        new Vector3(
            0.1f,
            0.1f,
            0.1f
        );

    [Header("Precio tienda")]
    public int shopPrice = 100;

    [Header("Reglas especiales")]
    public bool isBoss = false;
}