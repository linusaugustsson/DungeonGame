using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abillities/Barbarian/IgnorePain")]
public class IgnorePain_Ability : Ability
{
    [Space]
    public float healAmount = 5f;

    public override void ActivateServer(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        base.ActivateServer(player);
        Health _health = player.GetComponent<Health>();
        _health.onDamageTaken.AddListener(EndTrigger);
    }

    public override void ContiniousEffect(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        player.GetComponent<Health>().RestoreHealth(healAmount, player.transform);
    }
}
