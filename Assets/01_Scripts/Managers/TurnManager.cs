using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    private List<UnitStats> attackOrder = new();

    private UnitStats currentUnit;
    private List<UnitStats> allies;
    private List<UnitStats> enemies;
    private bool isCombatActive;
    private bool isTurn = false;
    private Coroutine turnTimeoutCoroutine;

    public bool IsCombatActive => isCombatActive;

    public void StartTurns(
        List<UnitStats> allies,
        List<UnitStats> enemies)
    {
        isCombatActive = true;
        this.allies = allies;
        this.enemies = enemies;

        ResetActionFlags(allies);
        ResetActionFlags(enemies);
        RefreshTurnOrder();

        StartTurn();
    }

    public void BeginNewWave(
        List<UnitStats> allies,
        List<UnitStats> enemies)
    {
        StartTurns(allies, enemies);
    }

    public void StopCombat()
    {
        isCombatActive = false;
    }

    public void UpdateCombatants(
        List<UnitStats> allies,
        List<UnitStats> enemies)
    {
        this.allies = allies;
        this.enemies = enemies;

        RefreshTurnOrder();
    }

    public void RefreshTurnOrder()
    {
        attackOrder.Clear();

        AddLivingUnits(attackOrder, allies);
        AddLivingUnits(attackOrder, enemies);

        attackOrder.Sort((a, b) =>
        {
            int speedCompare =
                b.speed.CompareTo(a.speed);

            if(speedCompare != 0)
                return speedCompare;

            if(a.faction == EnumFigthList.Faction.Ally &&
            b.faction == EnumFigthList.Faction.Enemy)
            {
                return -1;
            }

            if(a.faction == EnumFigthList.Faction.Enemy &&
            b.faction == EnumFigthList.Faction.Ally)
            {
                return 1;
            }

            return 0;
        });
    }

    void StartTurn()
    {
        if(!isCombatActive)
            return;

        // Si isTurn está activo pero currentUnit ya murió durante su propio turno
        // (ej: daño por reflejo/veneno), desbloquear el estado para continuar.
        if(isTurn && !UnitStats.IsCombatReady(currentUnit))
        {
            Debug.Log($"[TurnManager] {(currentUnit != null ? currentUnit.name : "null")} murió durante su turno — forzando avance.");
            CancelTurnTimeout();
            isTurn = false;
        }

        if(isTurn)
        {
            Debug.LogWarning($"[TurnManager] Lo intentó — turno ya activo para: {(currentUnit != null ? currentUnit.name : "null")} (isTurn={isTurn})");
            return;
        }

        currentUnit = GetNextUnit();

        if(currentUnit == null)
        {
            StartNewRound();
            Debug.Log("Primer nulo");
            currentUnit = GetNextUnit();

            if(currentUnit == null)
            {
                Debug.Log("Segundo nulo");
                return;
            }
        }

        Debug.Log("Turno de: " + currentUnit.name);
        isTurn = true;

        // Seguro: si el turno no termina en 10 seg, se fuerza EndTurn
        CancelTurnTimeout();
        turnTimeoutCoroutine = StartCoroutine(TurnTimeoutCoroutine(currentUnit));

        currentUnit.TakeTurn();
    }

    IEnumerator TurnTimeoutCoroutine(UnitStats unit)
    {
        yield return new WaitForSeconds(10f);

        if(isTurn && currentUnit == unit)
        {
            Debug.LogWarning($"[TurnManager] TIMEOUT — el turno de {(unit != null ? unit.name : "null")} no terminó en 10 seg. Forzando EndTurn.");
            EndTurn();
        }
    }

    void CancelTurnTimeout()
    {
        if(turnTimeoutCoroutine != null)
        {
            StopCoroutine(turnTimeoutCoroutine);
            turnTimeoutCoroutine = null;
        }
    }

    UnitStats GetNextUnit()
    {
        foreach(UnitStats unit in attackOrder)
        {
            if(!UnitStats.IsCombatReady(unit))
                continue;

            if(unit.hasActedThisRound)
                continue;

            return unit;
        }

        return null;
    }

    void StartNewRound()
    {
        if(!isCombatActive)
            return;

        Debug.Log("NUEVA RONDA");

        foreach(UnitStats unit in attackOrder)
        {
            if(!UnitStats.IsCombatReady(unit))
                continue;

            unit.hasActedThisRound = false;
        }

        RefreshTurnOrder();
    }

    public void EndTurn()
    {
        if(!isCombatActive)
            return;

        Debug.Log("Turno acabado");
        CancelTurnTimeout();
        isTurn = false;

        FightManager fightManager =
            FindFirstObjectByType<FightManager>();

        bool continueCombat =
            fightManager == null || fightManager.OnTurnEnded();

        if(!isCombatActive || !continueCombat)
            return;

        StartTurn();
    }

    public void RemoveUnit(UnitStats unit)
    {
        attackOrder.Remove(unit);
    }

    static void ResetActionFlags(List<UnitStats> units)
    {
        if(units == null)
            return;

        foreach(UnitStats unit in units)
        {
            if(!UnitStats.IsCombatReady(unit))
                continue;

            unit.hasActedThisRound = false;
        }
    }

    static void AddLivingUnits(
        List<UnitStats> destination,
        List<UnitStats> source)
    {
        if(source == null)
            return;

        foreach(UnitStats unit in source)
        {
            if(UnitStats.IsCombatReady(unit))
                destination.Add(unit);
        }
    }
}
