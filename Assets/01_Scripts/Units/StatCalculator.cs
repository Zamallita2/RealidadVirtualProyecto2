using UnityEngine;

public static class StatCalculator
{
    const float HealthPerLevel = 0.12f;
    const float StrengthPerLevel = 0.10f;
    const float SpeedPerLevel = 0.05f;

    public static void Recalculate(
        AdventurerData adv)
    {
        UnitStats baseStats =
            adv.prefab.GetComponent<UnitStats>();

        int level = adv.level;

        if (baseStats == null)
        {
            Debug.Log("Base stats nulas");
            return;
        }

        adv.maxHealth = baseStats.maxHealth * (1f + HealthPerLevel * (level - 1));
        adv.strength = Mathf.RoundToInt(baseStats.strength * (1f + StrengthPerLevel * (level - 1)));
        adv.speed = Mathf.Max(1, Mathf.RoundToInt(baseStats.speed * (1f + SpeedPerLevel * (level - 1))));
    }

    public static void RecalculateWithHealthPreserved(AdventurerData adv)
    {
        float hpPercent = adv.currentHealth / adv.maxHealth;

        Recalculate(adv);

        adv.currentHealth = adv.maxHealth * hpPercent;
    }
}