using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UnitStats : MonoBehaviour
{
    [Header("Stats")]
    public int level = 1;
    public float maxHealth = 10;
    public float currentHealth;

    public int strength = 2;
    public int speed = 5;

    [Header("State")]
    public bool isAlive = true;

    [Header("UI Health")]
    public Image healthFillImage;

    [Header("Battle")]
    public int lineNumber;
    public bool hasActedThisRound = false;
    [HideInInspector] public int partyIndex = -1;
    [HideInInspector] public GameObject sourceEnemyPrefab;

    protected FightManager fightManager;
    private TurnManager turnManager;

    [Header("Faction")]
    public EnumFigthList.Faction faction;

    private UnitCombat combat;
    private UnitStatus status;

    protected virtual void Awake()
    {
        status = GetComponent<UnitStatus>();
        combat = GetComponent<UnitCombat>();
        currentHealth = maxHealth;

        fightManager = FindFirstObjectByType<FightManager>();
        turnManager = FindFirstObjectByType<TurnManager>();

        UpdateHealthUI(); // 💛 inicializa bonito owo
    }

    // 💖 ACTUALIZA LA BARRA
    public void UpdateHealthUI()
    {
        if (healthFillImage == null)
            return;

        float fill = currentHealth / maxHealth;
        healthFillImage.fillAmount = Mathf.Clamp01(fill);
    }

    public void ApplyFromData(AdventurerData data)
    {
        level = data.level;
        maxHealth = data.maxHealth;
        currentHealth = data.currentHealth;
        strength = data.strength;
        speed = data.speed;
        isAlive = data.isAlive;

        UpdateHealthUI();
    }

    public void CopyToData(AdventurerData data)
    {
        data.level = level;
        data.maxHealth = maxHealth;
        data.currentHealth = currentHealth;
        data.strength = strength;
        data.speed = speed;
        data.isAlive = isAlive;
    }

    public static bool IsCombatReady(UnitStats unit)
    {
        return unit != null && unit.isAlive;
    }

    public void TakeTurn()
    {
        if(turnManager != null && !turnManager.IsCombatActive)
            return;

        if(!status.CanAct())
        {
            hasActedThisRound=true;
            turnManager.EndTurn();
            return;
        }

        hasActedThisRound = true;
        combat.TakeTurn();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;

        if(currentHealth > maxHealth)
            currentHealth = maxHealth;

        UpdateHealthUI(); // 💚 cuando cura
    }

    public virtual void TakeDamage(int damage)
    {
        if(!isAlive)
            return;

        if(status.currentStatus == EnumFigthList.StatusEffect.Weakness)
            damage = damage * 2;

        currentHealth -= damage;

        if(currentHealth <= 0)
        {
            currentHealth = 0;
            UpdateHealthUI();
            Die();
            return;
        }

        UpdateHealthUI(); // 💔 cuando recibe daño
    }

    protected virtual void Die()
    {
        isAlive = false;

        if(fightManager != null && fightManager.IsFightActive)
        {
            fightManager.UnitDied(this);
        }

        if(faction == EnumFigthList.Faction.Ally)
        {
            gameObject.SetActive(false);
            return;
        }

        Destroy(gameObject);
    }
}