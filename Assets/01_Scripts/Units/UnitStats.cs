using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UnitStats : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 10;
    public int currentHealth;

    public int strength = 2;
    public int speed = 5;

    [Header("Battle")]
    public int lineNumber;
    public bool hasActedThisRound = false;

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
    public void TakeTurn()
    {
        if(!status.CanAct())
        {
            turnManager.EndTurn();

            return;
        }

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
        currentHealth -= damage;

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        if(fightManager != null)
        {
            fightManager.UnitDied(this);
        }

        Destroy(gameObject);
    }
}