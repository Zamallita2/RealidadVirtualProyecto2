using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UnitCombat : MonoBehaviour
{
    [Header("Skills")]
    public List<BattleSkill> skills;

    [Header("Combat")]
    public bool isRanged = false;

    [Header("Ranged")]
    public GameObject projectilePrefab;

    public Transform projectileSpawnPoint;

    private List<UnitStats> currentTargets;

    private BattleSkill currentSkill;

    private FightManager fightManager;

    private UnitStats stats;

    private UnitMovement movement;

    private Animator animator;
    private TargetSelector targetSelector;

    private EnumFigthList.SkillCategory lastSkillType;

    void Awake()
    {
        fightManager =
            FindFirstObjectByType<FightManager>();

        stats = GetComponent<UnitStats>();

        movement = GetComponent<UnitMovement>();

        animator = GetComponent<Animator>();
        targetSelector = FindFirstObjectByType<TargetSelector>();
    }

    public void TakeTurn()
    {
        BattleSkill selectedSkill =
            ChooseSkill();

        if(selectedSkill == null)
        {
            Debug.LogWarning(name + " no tiene skills");

            fightManager.EndTurn();

            return;
        }

        UseSkill(selectedSkill);
    }

    void UseSkill(BattleSkill skill)
    {
        List<UnitStats> targets =
            targetSelector.GetTargets(
                stats,
                skill.targetType
            );

        if(targets == null || targets.Count == 0)
        {
            Debug.LogWarning("No hay targets");

            fightManager.EndTurn();

            return;
        }

        currentTargets = targets;

        currentSkill = skill;

        movement.SaveOriginalTransform();

        if(isRanged)
        {
            StartRangedAttack();
        }
        else
        {
            StartMeleeAttack();
        }
    }

    void StartMeleeAttack()
    {
        UnitStats target = currentTargets[0];

        Vector3 dir =
            (transform.position -
            target.transform.position).normalized;

        Vector3 targetPos =
            target.transform.position +
            dir * 0.2f;

        StartCoroutine(
            movement.MoveToPositionAndAttack(
                targetPos,
                () =>
                {
                    movement.PlayAttackAnimation();
                }
            )
        );
    }

    void StartRangedAttack()
    {
        movement.PlayAttackAnimation();
    }

    public void AttackAnimationFinished()
    {
        if(isRanged)
        {
            StartCoroutine(FinishRangedAttack());
        }
        else
        {
            foreach(UnitStats target in currentTargets)
            {
                if(target == null)
                    continue;

                ApplySkill(currentSkill, target);
            }

            StartCoroutine(
                movement.ReturnToOriginalPosition(
                    () =>
                    {
                        fightManager.EndTurn();
                    }
                )
            );
        }
    }

    IEnumerator FinishRangedAttack()
    {
        yield return new WaitForSeconds(0.5f);

        fightManager.EndTurn();
    }

    public void SpawnProjectile()
    {
        UnitStats target = currentTargets[0];

        GameObject obj =
            Instantiate(
                projectilePrefab,
                projectileSpawnPoint.position,
                Quaternion.identity
            );

        Projectile projectile =
            obj.GetComponent<Projectile>();

        projectile.Initialize(
            target.transform,
            () =>
            {
                foreach(UnitStats t in currentTargets)
                {
                    if(t == null)
                        continue;

                    ApplySkill(currentSkill, t);
                }
            });
    }

    void ApplySkill(
        BattleSkill skill,
        UnitStats target
    )
    {
        int amount =
            Mathf.RoundToInt(
                stats.strength *
                skill.strengthMultiplier
            );

        if(skill.isHealing)
        {
            target.Heal(amount);
        }
        else
        {
            target.TakeDamage(amount);
        }

        if(skill.applyEffect !=
        EnumFigthList.StatusEffect.None)
        {
            if(Random.value <= skill.effectChance)
            {
                UnitStatus targetStatus =
                    target.GetComponent<UnitStatus>();

                targetStatus.ApplyStatus(
                    skill.applyEffect,
                    skill.effectDuration
                );
            }
        }
    }

    BattleSkill ChooseSkill()
    {
        if(skills == null || skills.Count == 0)
        {
            return null;
        }

        List<BattleSkill> possibleSkills =
            new();

        foreach(BattleSkill skill in skills)
        {
            if(lastSkillType ==
            EnumFigthList.SkillCategory.Support &&
            skill.category ==
            EnumFigthList.SkillCategory.Support)
            {
                continue;
            }

            possibleSkills.Add(skill);
        }

        if(possibleSkills.Count == 0)
        {
            possibleSkills = skills;
        }

        BattleSkill selected =
            possibleSkills[
                Random.Range(
                    0,
                    possibleSkills.Count
                )
            ];

        lastSkillType = selected.category;

        return selected;
    }
}