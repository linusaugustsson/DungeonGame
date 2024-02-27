using FishNet;
using System.Collections.Generic;
using UnityEngine;

public class Ability : ScriptableObject, System.IComparable<Ability>
{
    public static List<Ability> abilityDatabase = new();
    public enum VFXLifeTime
    {
        SelfHandling,
        ActiveTimeOffset,
        DestroyAfter,
    }


    [Header("Abillity Varibles")]
    public string abilityName;
    public float castTime;
    public float cooldownTime;
    public float activeTime;
    public float activeInterval;
    public bool requireStandingStill = false;
    [Space]

    [Header("Ability Activation")]
    public bool useSimpleActivationLogic = true;
    public GameObject abilityActivationObject;
    public Vector3 abilityActivationOffset;



    [Header("Graphics")]
    public Sprite icon;

    [Header("SFX")]

    public AudioClip castSFX;

    [Header("Cast Visual Effect")]
    public Transform castingVisualEffect;
    [Header("Activation VFX")]
    public Transform activateVFX;
    public Vector3 activateVFXOffset;
    public VFXLifeTime activeVFXLifeTime;
    public float activeVFXlifeTimeValue;

    protected int abilityIndex => abilityDatabase.FindIndex((a) => a == this);



    internal bool endTriggered = false;

    public virtual void ActivateServer(SimplePlayerController player, AbilityHolder abilityHolder = null)
    {
        if (useSimpleActivationLogic && abilityActivationObject != null)
        {
            var ability = abilityDatabase[abilityIndex];
            GameObject newMageLight = Instantiate(ability.abilityActivationObject, player.transform.position + player.transform.TransformDirection(ability.abilityActivationOffset), Quaternion.Euler(player.transform.rotation.eulerAngles));
            InstanceFinder.ServerManager.Spawn(newMageLight, player.LocalConnection);
        }
    }

    public virtual void ContiniousEffect(SimplePlayerController player, AbilityHolder abilityHolder = null) { }

    public virtual void OnContiniousEffectEnd(SimplePlayerController player, AbilityHolder abilityHolder = null) { }

    public virtual void EndTrigger() { }

    public int CompareTo(Ability other)
    {
        return this.abilityName.CompareTo(other.abilityName);
    }
}
