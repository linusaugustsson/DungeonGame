using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abillities/Paladin/DivineLight")]
public class DivineLight_Ability : Ability
{
    public override void ActivateServer(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        GameObject newDivineLight = Instantiate(abilityActivationObject, player.transform.position + player.transform.forward * 0.15f + player.transform.up * 0.75f, Quaternion.Euler(player.transform.rotation.eulerAngles));
        newDivineLight.transform.rotation = Quaternion.Euler(player.transform.rotation.eulerAngles);
        FollowTransform followTransform = newDivineLight.GetComponent<FollowTransform>();
        if (followTransform != null)
        {
            followTransform.playerController = player;
            followTransform.followTarget = player.transform;
        }
        InstanceFinder.ServerManager.Spawn(newDivineLight, player.LocalConnection);
        Destroy(newDivineLight, activeTime);
    }
}

