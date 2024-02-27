using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abillities/Barbarian/Rage")]
public class Rage_Ability : Ability
{

    [Header("Enrage Settings")]
    [Range(-1, 1)]public float damageReduction = 0.5f;

    public override void ActivateServer(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        base.ActivateServer(player);
        Health _health = player.GetComponent<Health>();
        _health.onDamageTaken.AddListener(EndTrigger);
    }

    public override void ContiniousEffect(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        if (endTriggered == false)
        {
            Health _health = player.GetComponent<Health>();
            _health.SetDamageResistance(damageReduction);
        }
        else
        {
            abilityHolder.activeTime = 0;

            Health _health = player.GetComponent<Health>();
            _health.SetDamageResistance(0f);
            _health.onDamageTaken.RemoveListener(EndTrigger);
        }
    }

    public override void EndTrigger()
    {
        base.EndTrigger();
        endTriggered = true;
    }
}
