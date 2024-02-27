using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abillities/Paladin/BlessingOfRejuvination")]
public class BlessingOfRejuvination_ability : Ability
{
    [Space]
    public float healAmount = 3f;

    public override void ContiniousEffect(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        player.GetComponent<Health>().RestoreHealth(healAmount, player.transform);
    }
}
