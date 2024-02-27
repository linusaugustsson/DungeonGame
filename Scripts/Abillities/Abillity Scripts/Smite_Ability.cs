using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abillities/Paladin/Smite")]
public class Smite_Ability : Ability
{


    public override void ActivateServer(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        base.ActivateServer(player, abilityHolder);
    }
}
