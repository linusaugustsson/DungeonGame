using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct stat
{
    public float score;
    internal float minScore;
    internal float maxScore;
    public float AttributeToStatScaling;
}

[RequireComponent(typeof(Attributes))]
public class Stats : MonoBehaviour
{
    [Header("STATS")]
    public stat health;
    [Space]
    public stat movementSpeed;
    [Space]
    public stat attackSpeed;
    [Space]
    public stat attackPower;
    [Space]
    public stat abilityPower;
    [Space]
    public stat cooldownReduction;
    [Space]
    public stat abilityCastingSpeed;

    internal Attributes attributes;

    [Space(25f)]
    public float movementSpeedBaseValue = 2f;
    public float attackSpeedBaseValue = 1f;
    public float attackpowerBaseValue = 0f;
    public float abilitypowerBaseValue = 1f;
    public float cooldownReductionBaseValue = 1;
    public float abilityCastingSpeedBaseValue = 1;

    private void Awake()
    {
        attributes = GetComponent<Attributes>();

        /// Health
        health.maxScore = attributes.Constitution.score * 10f;
        health.score = health.maxScore;
        health.minScore = 0f;

        /// Movement Speed
        movementSpeed.maxScore = movementSpeedBaseValue + attributes.Dexterity.score * 0.05f;
        movementSpeed.score = movementSpeed.maxScore;
        movementSpeed.minScore = 0;

        /// Attack Speed
        attackSpeed.maxScore = attackSpeedBaseValue - attributes.Dexterity.score * 0.05f;
        attackSpeed.score = attackSpeed.maxScore;
        attackSpeed.minScore = 0;

        /// Attack Power
        attackPower.maxScore = attackpowerBaseValue + attributes.Strength.score * 1f;
        attackPower.score = attackPower.maxScore;
        attackPower.minScore = 0;

        /// Ability Power
        abilityPower.maxScore = abilitypowerBaseValue + attributes.Intelligence.score * 1f;
        abilityPower.score = abilityPower.maxScore;
        abilityPower.minScore = 0;

        /// Cooldown Reduction
        cooldownReduction.maxScore = cooldownReductionBaseValue + attributes.Wisdom.score * 1f;
        cooldownReduction.score = cooldownReduction.maxScore;
        cooldownReduction.minScore = 0;

        /// Ability Casting Speed
        abilityCastingSpeed.maxScore = abilityCastingSpeedBaseValue + attributes.Wisdom.score * 1f;
        abilityCastingSpeed.score = abilityCastingSpeed.maxScore;
        abilityCastingSpeed.minScore = 0;
    }
}
