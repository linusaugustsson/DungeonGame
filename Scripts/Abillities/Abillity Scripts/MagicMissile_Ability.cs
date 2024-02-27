using FishNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Abillities/Wizard/MagicMissle")]
public class MagicMissile_Ability : Ability
{
    [Header("Magic Missle Settings")]
    public int numberOfMissles = 3;
    public float missileOffsetMax = 0.05f;
    public float missileOffsetMin = 0.15f;
    public float delayMax = 0.25f;
    public float delayMin = 0.15f;

    public override void ActivateServer(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        base.ActivateServer(player, abilityHolder);
        player.StartCoroutine(FireMagicMissles(player));
    }

    IEnumerator FireMagicMissles(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        bool spawnRight = true;
        Vector3 offset = new Vector3(0f, 0f, 0f);
        int j = 1;

        for (int i = 0; i < 3; i++)
        {
            float zOffset = Random.Range(missileOffsetMin, missileOffsetMax);

            if (i != 0)
            {
                if (spawnRight)
                {
                    offset = new Vector3(0f, 0f, zOffset * j);
                    spawnRight = false;
                }
                else
                {
                    offset = new Vector3(0f, 0f, -zOffset * j);
                    j = i;
                    spawnRight = true;
                }
            }



            var ability = Ability.abilityDatabase[abilityIndex];
            GameObject newMageLight = Instantiate(ability.abilityActivationObject, player.transform.position + player.transform.TransformDirection(ability.abilityActivationOffset) + offset, Quaternion.Euler(player.transform.rotation.eulerAngles));
            InstanceFinder.ServerManager.Spawn(newMageLight, player.LocalConnection);
          


            float delay = Random.Range(delayMin, delayMax);
            yield return new WaitForSeconds(delay);
        }

        yield break;
    }
}
