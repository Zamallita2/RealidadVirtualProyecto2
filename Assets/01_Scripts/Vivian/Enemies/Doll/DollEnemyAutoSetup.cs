using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(UnitStats))]
[RequireComponent(typeof(UnitCombat))]
[RequireComponent(typeof(UnitMovement))]
[RequireComponent(typeof(UnitStatus))]
public class DollEnemyAutoSetup : MonoBehaviour
{
    [Header("Stats - Muñeca Sin Ojos")]
    public int vida = 150;
    public int ataque = 20;
    public int velocidad = 6;

    [Tooltip("Solo queda como referencia visual porque tu sistema actual no tiene defensa todavía.")]
    public int defensaVisual = 10;

    [Header("Movimiento lento")]
    public float velocidadMovimiento = 1.15f;
    public float distanciaMinimaAtaque = 0.55f;
    public Vector3 rotacionModelo;

    [Header("Skills")]
    [Range(0f, 1f)] public float probabilidadSusurro = 0.35f;
    public int duracionParalisis = 1;
    public bool configurarSkillsAutomaticamente = true;

    void Reset()
    {
        ConfigurarPrefab();
    }

    void OnValidate()
    {
        if(!Application.isPlaying)
            ConfigurarPrefab();
    }

    [ContextMenu("Configurar Muñeca Sin Ojos")]
    public void ConfigurarPrefab()
    {
        UnitStats stats = GetComponent<UnitStats>();
        UnitCombat combat = GetComponent<UnitCombat>();
        UnitMovement movement = GetComponent<UnitMovement>();
        UnitStatus status = GetComponent<UnitStatus>();

        if(stats != null)
        {
            stats.maxHealth = vida;
            stats.currentHealth = vida;
            stats.strength = ataque;
            stats.speed = velocidad;
            stats.faction = EnumFigthList.Faction.Enemy;
            stats.isAlive = true;
        }

        if(movement != null)
        {
            movement.moveSpeed = velocidadMovimiento;
            movement.distanciaMinima = distanciaMinimaAtaque;
            movement.modelRotationOffset = rotacionModelo;
        }

        if(status != null)
        {
            status.currentStatus = EnumFigthList.StatusEffect.None;
            status.statusTurnsRemaining = 0;
        }

        if(combat != null)
        {
            combat.isRanged = false;

            if(configurarSkillsAutomaticamente)
            {
                combat.skills = CrearSkillsMuneca();
            }
        }
    }

    List<BattleSkill> CrearSkillsMuneca()
    {
        BattleSkill aranazoPorcelana = new BattleSkill
        {
            skillName = "Arañazo de Porcelana",
            category = EnumFigthList.SkillCategory.Damage,
            isHealing = false,
            isAOE = false,
            strengthMultiplier = 1f,
            targetType = EnumFigthList.TargetType.RandomEnemy,
            applyEffect = EnumFigthList.StatusEffect.None,
            effectChance = 0f,
            effectDuration = 0
        };

        BattleSkill susurroRoto = new BattleSkill
        {
            skillName = "Susurro Roto",
            category = EnumFigthList.SkillCategory.State,
            isHealing = false,
            isAOE = false,
            strengthMultiplier = 0.65f,
            targetType = EnumFigthList.TargetType.RandomEnemyWithoutStatus,
            applyEffect = EnumFigthList.StatusEffect.Paralysis,
            effectChance = probabilidadSusurro,
            effectDuration = duracionParalisis
        };

        return new List<BattleSkill>
        {
            aranazoPorcelana,
            susurroRoto
        };
    }
}
