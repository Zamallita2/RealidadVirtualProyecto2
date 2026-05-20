using System.Collections.Generic;
using UnityEngine;

public static class TargetSystem
{
    public static List<UnitStats> GetTargets(
        UnitStats caster,
        EnumFigthList.TargetType targetType,
        List<UnitStats> allies,
        List<UnitStats> enemies)
    {
        List<UnitStats> result = new();

        switch(targetType)
        {
            case EnumFigthList.TargetType.RandomEnemy:

                result.Add(GetFrontPriorityRandom(enemies));
                break;

            case EnumFigthList.TargetType.LowestHealthEnemy:

                result.Add(GetLowestHealth(enemies));
                break;

            case EnumFigthList.TargetType.LowestHealthAlly:

                result.Add(GetLowestHealth(allies));
                break;

            case EnumFigthList.TargetType.AllEnemies:

                result.AddRange(GetLivingUnits(enemies));
                break;

            case EnumFigthList.TargetType.RandomEnemyWithoutStatus:

                result.Add(GetEnemyWithoutStatus(enemies));
                break;
        }

        result.RemoveAll(x => !UnitStats.IsCombatReady(x));

        return result;
    }

    static List<UnitStats> GetLivingUnits(List<UnitStats> units)
    {
        List<UnitStats> living = new();

        foreach(UnitStats unit in units)
        {
            if(UnitStats.IsCombatReady(unit))
                living.Add(unit);
        }

        return living;
    }

    static UnitStats GetEnemyWithoutStatus(
        List<UnitStats> units)
    {
        List<UnitStats> possibleTargets = new();

        foreach(UnitStats unit in units)
        {
            if(!UnitStats.IsCombatReady(unit))
                continue;

            UnitStatus status =
                unit.GetComponent<UnitStatus>();

            if(status.currentStatus !=
                EnumFigthList.StatusEffect.None)
            {
                continue;
            }

            possibleTargets.Add(unit);
        }

        if(possibleTargets.Count == 0)
            return null;

        possibleTargets.Sort((a, b) =>
            a.lineNumber.CompareTo(b.lineNumber));

        int maxIndex =
            Mathf.Min(2, possibleTargets.Count);

        return possibleTargets[
            Random.Range(0, maxIndex)
        ];
    }

    static UnitStats GetFrontPriorityRandom(
        List<UnitStats> units)
    {
        List<UnitStats> living = GetLivingUnits(units);

        if(living.Count == 0)
            return null;

        living.Sort((a, b) =>
            a.lineNumber.CompareTo(b.lineNumber));

        int maxIndex =
            Mathf.Min(2, living.Count);

        return living[
            Random.Range(0, maxIndex)
        ];
    }

    static UnitStats GetLowestHealth(
        List<UnitStats> units)
    {
        List<UnitStats> living = GetLivingUnits(units);

        if(living.Count == 0)
            return null;

        UnitStats lowest = living[0];

        foreach(UnitStats unit in living)
        {
            if(unit.currentHealth <
                lowest.currentHealth)
            {
                lowest = unit;
            }
        }

        return lowest;
    }
}
