using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abillities/Warlock/HellfireBlast")]
public class HellfireBlast_Ability : Ability
{
    public override void ActivateServer(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        base.ActivateServer(player, abilityHolder);
    }
}
