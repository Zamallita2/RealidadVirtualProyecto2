using UnityEngine;

public enum GachaRarity
{
    Comun,
    Raro,
    Epico,
    Legendario
}

[CreateAssetMenu(fileName = "EnemyGachaData", menuName = "Gacha/Enemy Gacha Data")]
public class EnemyGachaData : ScriptableObject
{
    [Header("ID único")]
    public string enemyId;

    [Header("Información")]
    public string enemyName;
    public GachaRarity rarity;

    [TextArea(2, 5)]
    public string description;

    [Header("Stats")]
    public int hp;
    public int attack;
    public int defense;
    public int speed;

    [Header("Habilidad")]
    public string skillName;

    [TextArea(2, 4)]
    public string skillDescription;

    [Header("Visuales")]
    public Sprite cardFrontSprite;
    public Sprite cardBackSprite;

    [Header("Paneles ya hechos")]
    public Sprite newRewardPanelSprite;
    public Sprite duplicateRewardPanelSprite;

    [Header("Prefab para salas")]
    public GameObject enemyPrefab;

    [Header("Duplicado")]
    public int duplicateFragments = 10;
}