using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abillities/Wizard/Fireball")]
public class Fireball_Ability : Ability
{
    public override void ActivateServer(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        base.ActivateServer(player, abilityHolder);
    }
}
