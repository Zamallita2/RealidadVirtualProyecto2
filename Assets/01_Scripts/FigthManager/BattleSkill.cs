using UnityEngine;

[System.Serializable]
public class BattleSkill
{
    public string skillName;

    [Header("General")]
    public EnumFigthList.SkillCategory category;

    public bool isHealing;

    public bool isAOE;

    [Header("Power")]
    public float strengthMultiplier = 1f;

    [Header("Target")]
    public EnumFigthList.TargetType targetType;

    [Header("Effect")]
    public EnumFigthList.StatusEffect applyEffect = EnumFigthList.StatusEffect.None;

    [Range(0f, 1f)]
    public float effectChance = 0.3f;
    [Header("Status")]
    public int effectDuration = 1;
}