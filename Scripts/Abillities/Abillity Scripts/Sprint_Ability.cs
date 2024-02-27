using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abillities/Thief/Sprint")]
public class Sprint_Ability : Ability
{
    [Header("Haste Settings")]
    public float newMovementSpeed = 3.75f;

    public override void ContiniousEffect(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        player.transitionSpeed = newMovementSpeed;
    }

    public override void OnContiniousEffectEnd(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        player.transitionSpeed = 2.5f;
    }
}
