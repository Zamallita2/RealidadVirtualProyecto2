using System.Collections.Generic;
using UnityEngine;

public class EnumFigthList : MonoBehaviour
{
    public enum Faction
    {
        Ally,
        Enemy
    }
    public enum StatusEffect
    {
        None,
        Paralysis
    }
    public enum TargetType
    {
        RandomEnemy,
        LowestHealthEnemy,

        LowestHealthAlly,

        AllEnemies,
        RandomEnemyWithoutStatus
    }
    public enum SkillCategory
    {
        Damage,
        Support,
        State
    }
}