using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UnitStats : MonoBehaviour
{
    [Header("Stats")]
    public int level = 1;
    public int maxHealth = 10;
    public int currentHealth;

    public int strength = 2;
    public int speed = 5;

    [Header("State")]
    public bool isAlive = true;

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
    }

    public void ApplyFromData(AdventurerData data)
    {
        level = data.level;
        maxHealth = data.maxHealth;
        currentHealth = data.currentHealth;
        strength = data.strength;
        speed = data.speed;
        isAlive = data.isAlive;
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
        {
            currentHealth = maxHealth;
        }
    }

    public virtual void TakeDamage(int damage)
    {
        if(!isAlive)
            return;

        currentHealth -= damage;

        if(currentHealth <= 0)
        {
            Die();
        }
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
