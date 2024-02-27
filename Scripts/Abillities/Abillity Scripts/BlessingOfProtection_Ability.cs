using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abillities/Paladin/BlessingOfProtection")]
public class BlessingOfProtection_Ability : Ability
{
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
            _health.SetDamageResistance(1f);
        }
        else
        {
            abilityHolder.activeTime = 0;

            Health _health = player.GetComponent<Health>();
            _health.SetDamageResistance(0);
            _health.onDamageTaken.RemoveListener(EndTrigger);
        }
    }

    public override void EndTrigger()
    {
        base.EndTrigger();
        endTriggered = true;
    }
}
