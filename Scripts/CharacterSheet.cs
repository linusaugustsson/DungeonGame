using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSheet : MonoBehaviour
{
    public WeaponData currentWeapon;
    public Class characterClass;

    private void Awake()
    {
        GetComponent<SimplePlayerController>().OnClientInitalized += OnClientInitalized;
    }

    private void OnClientInitalized(NetworkPlayerData client)
    {
            characterClass = Class.classDatabase[client.selectedClass];
            currentWeapon = characterClass.defaultWeapon;
    }
}
