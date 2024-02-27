using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abillities/Paladin/LayOnHands")]
public class LayOnHands_Ability : Ability
{
    [Header("Healing Settings")]
    public float healAmount;


    public override void ActivateServer(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        base.ActivateServer(player);
        player.GetComponent<Health>().RestoreHealth(healAmount, player.transform);
    }
}
