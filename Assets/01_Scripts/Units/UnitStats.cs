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
    [Header("Faction")]
    public EnumFigthList.Faction faction;

    [Header("Skills")]
    public List<BattleSkill> skills;

    [Header("Status")]
    public EnumFigthList.StatusEffect currentStatus;
    private EnumFigthList.SkillCategory lastSkillType;
    [Header("Status Turns")]
    public int statusTurnsRemaining = 0;
    [Header("Combat Animation")]
    public bool isRanged = false;

    public float moveSpeed = 4f;

    private Vector3 originalPosition;

    private Animator animator;

    private List<UnitStats> currentTargets;

    private BattleSkill currentSkill;
    [Header("Ranged")]
    public GameObject projectilePrefab;

    public Transform projectileSpawnPoint;
    private Quaternion originalRotation;
    [Header("Rotation")]
    public Vector3 modelRotationOffset;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        fightManager = FindFirstObjectByType<FightManager>();
    }
    public void TakeTurn()
    {
        bool skippedTurn = false;

        if(currentStatus == EnumFigthList.StatusEffect.Paralysis)
        {
            if(Random.value <= 0.5f)
            {
                Debug.Log(name + " está paralizado");

                skippedTurn = true;
            }
        }

        // Reducir duración DESPUÉS del efecto
        ProcessStatusTurn();

        if(skippedTurn)
        {
            fightManager.EndTurn();
            return;
        }

        BattleSkill selectedSkill = ChooseSkill();

        if(selectedSkill == null)
        {
            Debug.LogWarning(name + " no tiene skills");

            fightManager.EndTurn();
            return;
        }

        UseSkill(selectedSkill);
    }
    public void ProcessStatusTurn()
    {
        if(currentStatus == EnumFigthList.StatusEffect.None)
            return;

        statusTurnsRemaining--;

        if(statusTurnsRemaining <= 0)
        {
            currentStatus = EnumFigthList.StatusEffect.None;

            Debug.Log(name + " ya no tiene estado");
        }
    }
    void UseSkill(BattleSkill skill)
    {
        List<UnitStats> targets =
            fightManager.GetTargets(this, skill.targetType);

        if(targets == null || targets.Count == 0)
        {
            Debug.LogWarning("No hay targets");

            fightManager.EndTurn();
            return;
        }

        currentTargets = targets;
        currentSkill = skill;

        originalPosition = transform.position;
        originalRotation = transform.rotation;

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
            (transform.position - target.transform.position).normalized;

        Vector3 targetPos =
            target.transform.position + dir * 0.2f;

        StartCoroutine(
            MoveToPositionAndAttack(targetPos)
        );
    }
    void StartRangedAttack()
    {
        animator.SetTrigger("Attack");
    }
    IEnumerator MoveToPositionAndAttack(Vector3 targetPos)
    {
        animator.SetBool("IsWalking", true);
        Vector3 lookDir =
            targetPos - transform.position;

        lookDir.y = 0f;

        if(lookDir != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(lookDir);

            transform.rotation =
                targetRotation *
                Quaternion.Euler(modelRotationOffset);
        }

        while(Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    targetPos,
                    moveSpeed * Time.deltaTime
                );

            yield return null;
        }

        animator.SetBool("IsWalking", false);

        animator.SetTrigger("Attack");
    }
    IEnumerator ReturnToOriginalPosition()
    {
        animator.SetBool("IsWalking", true);
        Vector3 lookDir =
            originalPosition - transform.position;

        lookDir.y = 0f;

        if(lookDir != Vector3.zero)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(lookDir);

            transform.rotation =
                targetRotation *
                Quaternion.Euler(modelRotationOffset);
        }

        while(Vector3.Distance(transform.position, originalPosition) > 0.1f)
        {
            transform.position =
                Vector3.MoveTowards(
                    transform.position,
                    originalPosition,
                    moveSpeed * Time.deltaTime
                );

            yield return null;
        }

        animator.SetBool("IsWalking", false);

        yield return new WaitForSeconds(0.2f);
        transform.position = originalPosition;

        float timer = 0f;
        float rotateDuration = 0.25f;

        Quaternion startRotation =
            transform.rotation;

        while(timer < rotateDuration)
        {
            timer += Time.deltaTime;

            transform.rotation =
                Quaternion.Slerp(
                    startRotation,
                    originalRotation,
                    timer / rotateDuration
                );

            yield return null;
        }

        transform.rotation = originalRotation;

        yield return new WaitForSeconds(0.5f);

        fightManager.EndTurn();
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

            StartCoroutine(ReturnToOriginalPosition());
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
    void ApplySkill(BattleSkill skill, UnitStats target)
    {
        int amount =
            Mathf.RoundToInt(strength * skill.strengthMultiplier);

        if(skill.isHealing)
        {
            target.Heal(amount);
        }
        else
        {
            target.TakeDamage(amount);
        }

        // Efecto
        if(skill.applyEffect != EnumFigthList.StatusEffect.None)
        {
            if(Random.value <= skill.effectChance)
            {
                target.currentStatus = skill.applyEffect;
                target.statusTurnsRemaining = skill.effectDuration;
            }
        }
    }
    BattleSkill ChooseSkill()
    {
        if(skills == null || skills.Count == 0)
        {
            return null;
        }

        List<BattleSkill> possibleSkills = new();

        foreach(BattleSkill skill in skills)
        {
            if(lastSkillType == EnumFigthList.SkillCategory.Support &&
            skill.category == EnumFigthList.SkillCategory.Support)
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
            possibleSkills[Random.Range(0, possibleSkills.Count)];

        lastSkillType = selected.category;

        return selected;
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