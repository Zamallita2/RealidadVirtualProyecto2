using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    private List<UnitStats> attackOrder = new();

    private UnitStats currentUnit;
    private List<UnitStats> allies;
    private List<UnitStats> enemies;

    public void StartTurns(
    List<UnitStats> allies,
    List<UnitStats> enemies)
    {
        this.allies = allies;
        this.enemies = enemies;

        RefreshTurnOrder();

        StartTurn();
    }
    public void RefreshTurnOrder()
    {
        attackOrder.Clear();

        attackOrder.AddRange(allies);
        attackOrder.AddRange(enemies);

        attackOrder.RemoveAll(x => x == null);

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
        currentUnit = GetNextUnit();

        if(currentUnit == null)
        {
            StartNewRound();

            currentUnit = GetNextUnit();

            if(currentUnit == null)
                return;
        }

        Debug.Log("Turno de: " + currentUnit.name);

        currentUnit.hasActedThisRound = true;

        currentUnit.TakeTurn();
    }

    UnitStats GetNextUnit()
    {
        foreach(UnitStats unit in attackOrder)
        {
            if(unit == null)
                continue;

            if(unit.hasActedThisRound)
                continue;

            return unit;
        }

        return null;
    }

    void StartNewRound()
    {
        Debug.Log("NUEVA RONDA");

        foreach(UnitStats unit in attackOrder)
        {
            if(unit == null)
                continue;

            unit.hasActedThisRound = false;
        }

        RefreshTurnOrder();
    }

    public void EndTurn()
    {
        Debug.Log("Turno acabado");
        StartTurn();
    }

    public void RemoveUnit(UnitStats unit)
    {
        attackOrder.Remove(unit);
    }
}