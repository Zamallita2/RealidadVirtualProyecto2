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

                result.AddRange(enemies);
                break;

            case EnumFigthList.TargetType.RandomEnemyWithoutStatus:

                result.Add(GetEnemyWithoutStatus(enemies));
                break;
        }

        result.RemoveAll(x => x == null);

        return result;
    }

    static UnitStats GetEnemyWithoutStatus(
        List<UnitStats> units)
    {
        List<UnitStats> possibleTargets = new();

        foreach(UnitStats unit in units)
        {
            if(unit == null)
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
        if(units.Count == 0)
            return null;

        units.Sort((a, b) =>
            a.lineNumber.CompareTo(b.lineNumber));

        int maxIndex =
            Mathf.Min(2, units.Count);

        return units[
            Random.Range(0, maxIndex)
        ];
    }

    static UnitStats GetLowestHealth(
        List<UnitStats> units)
    {
        if(units.Count == 0)
            return null;

        UnitStats lowest = units[0];

        foreach(UnitStats unit in units)
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