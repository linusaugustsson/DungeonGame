using System;
using System.Collections;
using System.Collections.Generic;
using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AbilityHolder : object
{
    public Ability ability;
    public float cooldownTime;
    public float activeTime;
    internal float activeIntervalTimer;

    public enum abillityState { ready, casting, active, cooldown };
    public abillityState state = abillityState.ready;

    internal bool isSelected = false;

    internal BuffDeBuffElement buffDeBuffElement;

    internal void Activate()
    {
        activeTime = ability.activeTime;
        activeIntervalTimer = ability.activeInterval;
        state = abillityState.active;
    }

    public void Reset()
    {
        state = abillityState.ready;
        cooldownTime = 0;
        activeTime = 0;
        activeIntervalTimer = 0;

    }
}

public class AbillityUser : NetworkBehaviour
{
    static List<Ability> abilityDatabase => Ability.abilityDatabase;
    public List<Ability> abillities = new List<Ability>();

    public Action<WeaponData> OnWeaponAttack;


    [Space]
    private List<AbilityHolder> abilityHolder = new List<AbilityHolder>();

    internal AbilityHolder selectedAbility;
    internal int selectedAbilityIndex;
    internal AbilityHolder activatedAbility;

    public AudioClip genericAbilityCastSFX;

    SimplePlayerController playerController;

    Coroutine performAbilityCoroutine;

    internal Transform castingVisualEffect;


    [Space, Header("Weapon Attack/Ability")]
    public GameObject basicAttackEffect;
    float nextWeaponAttack;

    private CharacterSheet characterSheet;
    private WeaponData currentWeapon => characterSheet.currentWeapon;

    //Parry
    float parryCooldownTimer;
    bool CanParry => !parrying && parryCooldownTimer < Time.time;
    [SyncVar(WritePermissions = WritePermission.ClientUnsynchronized)] public bool parrying;

    private AudioSource audioSource;
    private HumanoidSpriteAnimator spriteAnimator;

    
    

    public override void OnStartClient()
    {
        base.OnStartClient();
    }


    public void WeaponAttack()
    {
        if (nextWeaponAttack > Time.time) return;

        if (activatedAbility != null && activatedAbility.state == AbilityHolder.abillityState.casting) return;

        OnWeaponAttack?.Invoke(currentWeapon);

        ServerTriggerWeaponAttack();

        audioSource.PlayOneShot(currentWeapon.weaponType.GetRandomWeaponSound(WeaponSounds.Swing));

        nextWeaponAttack = Time.time + currentWeapon.cooldown;
        StartCoroutine(WeaponAttackRoutine(0.35f));
    }

    [ServerRpc]
    void ServerTriggerWeaponAttack()
    {
        ClientTriggerWeaponAttack();
    }

    [ObserversRpc(ExcludeOwner = true)]
    void ClientTriggerWeaponAttack()
    {
        OnWeaponAttack?.Invoke(currentWeapon);
    }

    IEnumerator WeaponAttackRoutine(float delay)
    {
        yield return new WaitForSeconds(delay); //Remove when animations are in place
        Quaternion snappedRotation = Quaternion.Euler(0f, Mathf.Round(transform.rotation.eulerAngles.y / 90f) * 90f, 0f);
        var attackObj = Instantiate(basicAttackEffect, transform.position + (snappedRotation * Vector3.forward * 0.5f), snappedRotation).GetComponent<AttackVolume>();
        var s = attackObj.transform.localScale;
        attackObj.transform.localScale = new Vector3(currentWeapon.width, s.y, currentWeapon.reach);
        attackObj.weaponData = currentWeapon;
        attackObj.sender = transform;
    }


    public void Parry()
    {
        if (CanParry)
        {
            StartCoroutine(ParryWeaponLerp(currentWeapon.parryTransitionTime));
            parrying = !IsServer;
            ParryServer(currentWeapon.parryDuration);
        }
    }


    IEnumerator ParryWeaponLerp(float transitionTime)
    {
        var viewSprite = spriteAnimator.viewSprite;
        float parryTransitionTimer = Time.time + transitionTime;
        float parryTimer = Time.time + currentWeapon.parryDuration;
        Quaternion orig = viewSprite.transform.rotation;
        Quaternion destination = orig * Quaternion.Euler((currentWeapon.parryRotation));
        Vector3 origPos = viewSprite.transform.localPosition;
        Vector3 destinationPos = currentWeapon.parryPosition;
        float t = 0f;

        var viewSpriteOscillate = viewSprite.GetComponent<Oscillate>();
        viewSpriteOscillate.enabled = false;
        while (Time.time < parryTransitionTimer)
        {
            t = 1 - ((parryTransitionTimer - Time.time) / transitionTime);
            viewSprite.transform.localPosition = Vector3.Lerp(origPos, destinationPos, t);
            viewSprite.transform.rotation = Quaternion.Slerp(orig, destination, t);
            yield return new WaitForEndOfFrame();
        }
        viewSprite.transform.rotation = destination;
        viewSprite.transform.localPosition = destinationPos;


        yield return new WaitForSeconds(parryTimer - Time.time - transitionTime);

        parryTransitionTimer = Time.time + transitionTime;

        while (Time.time < parryTransitionTimer)
        {
            t = 1 - ((parryTransitionTimer - Time.time) / transitionTime);
            viewSprite.transform.localPosition = Vector3.Lerp(destinationPos, origPos, t);
            viewSprite.transform.rotation = Quaternion.Slerp(destination, orig, t);
            yield return new WaitForEndOfFrame();
        }
        viewSpriteOscillate.enabled = true;
        parryCooldownTimer = Time.time + currentWeapon.parryCooldown;
        viewSprite.transform.rotation = orig;
        viewSprite.transform.localPosition = origPos;

    }



    [ServerRpc(RunLocally = false, RequireOwnership = true)]
    public void ParryServer(float duration)
    {
        if (!parrying)
        {
            parrying = true;
            StartCoroutine(DeactivateParryAfterDuration(duration));
        }
    }

    IEnumerator DeactivateParryAfterDuration(float parryDuration)
    {
        yield return new WaitForSeconds(parryDuration);
        parrying = false;
    }


    List<int> abilitieIndecies;
    [ServerRpc]
    void SyncAbilities(List<int> abilitieIndecies)
    {
        this.abilitieIndecies = abilitieIndecies;
        foreach (var abilityIndex in abilitieIndecies)
        {
            Ability ability = abilityDatabase[abilityIndex];
            AbilityHolder abillityState = new AbilityHolder
            {
                ability = ability,
                cooldownTime = ability.cooldownTime,
                activeTime = ability.activeTime
            };
            abilityHolder.Add(abillityState);
        }
    }

    private void Awake()
    {
        playerController = GetComponent<SimplePlayerController>();
        audioSource = GetComponent<AudioSource>();
        spriteAnimator = GetComponent<HumanoidSpriteAnimator>();
        characterSheet = GetComponent<CharacterSheet>();

    }

    void Start()
    {
        castingVisualEffect = null;
        GetComponent<Health>().OnDeath += (h) =>
        {
            InterruptAbilityCasting();
            ResetCooldown();
        };


        //if (abilityDatabase.Count == 0)
        //{

        //    abilityDatabaseFetched = true;

        //    if (abilityHolder.Count == 0 && abilitieIndecies != null)
        //    {
        //        foreach (var abilityIndex in abilitieIndecies)
        //        {
        //            Ability ability = abilityDatabase[abilityIndex];
        //            AbilityHolder abillityState = new AbilityHolder
        //            {
        //                ability = ability,
        //                activeTime = ability.activeTime
        //            };
        //            abilityHolder.Add(abillityState);
        //        }
        //    }

        //}

    }

    private void ResetCooldown()
    {
        foreach (var ability in abilityHolder)
        {
            ability.Reset();
        }
    }

    public void InterruptAbilityCastOnMove()
    {
        if (activatedAbility != null && activatedAbility.ability.requireStandingStill)
        {
            playerController.playerHUD.CloseActionBar();
            InterruptAbilityCasting();
        }
    }

    [ServerRpc]
    public void Server_InterruptAbilityCast()
    {
        Client_InterruptAbilityCast();
    }

    [ObserversRpc(ExcludeOwner = true)]
    public void Client_InterruptAbilityCast()
    {
        InterruptAbilityCasting();
    }

    public void SelectAbillity(int index)
    {
        if (abilityHolder.Count > index && abilityHolder[index] != null)
        {
            selectedAbilityIndex = abilityDatabase.FindIndex((a) => a == abilityHolder[index].ability);
            if(selectedAbilityIndex == -1)
            {
                throw new Exception($"Tried to select {abilityHolder[index].ability.abilityName}, it is not addressable/have the ability lable as an Ability.");
            }
            selectedAbility = abilityHolder[index];

            for (int i = 0; i < abilityHolder.Count; i++)
            {
                abilityHolder[i].isSelected = false;
            }

            abilityHolder[index].isSelected = true;
        }
    }

    public void ActivateAbility()
    {
        if (selectedAbility == null) return;

        if (activatedAbility != null && activatedAbility.state == AbilityHolder.abillityState.casting)
        {
            if (performAbilityCoroutine != null)
            {
                InterruptAbilityCasting(activatedAbility != selectedAbility);
                activatedAbility.state = AbilityHolder.abillityState.ready;
                playerController.playerHUD.CloseActionBar();
                
                if (activatedAbility == selectedAbility)
                {
                    return;
                }
            }
        }

        if (selectedAbility.state == AbilityHolder.abillityState.ready)
        {
            activatedAbility = selectedAbility;

            AudioClip castSFX = activatedAbility.ability.castSFX != null ? activatedAbility.ability.castSFX : genericAbilityCastSFX;
            playerController.audioSoruce.PlayOneShot(castSFX);
            PlayCastSoundServer(selectedAbilityIndex);

            if (activatedAbility.ability.castTime > 0)
            {
                /// Fail safe
               // InterruptAbilityCasting();

                performAbilityCoroutine = StartCoroutine(PerformAbility());
            }
            else
            {
                activatedAbility.Activate();
                TriggerAbilityEffect(selectedAbilityIndex);
            }
        }
    }

    public void InterruptAbilityCasting(bool onlyInteruptLocally = false)
    {
        if(IsOwner && !onlyInteruptLocally)
        {
            Server_InterruptAbilityCast();
        }
        if (performAbilityCoroutine != null)
        {
            StopCoroutine(performAbilityCoroutine);
        }

        if (castingVisualEffect != null)
        {
            Destroy(castingVisualEffect.gameObject);
        }
    }

    [ServerRpc]
    void ServerRPC_CreateCastingVisual(int abilityIndex)
    {
        ClientRPC_CreateCastingVisual(abilityIndex);
    }

    [ObserversRpc(RunLocally = true)]
    void ClientRPC_CreateCastingVisual(int abilityIndex)
    {
        Ability ability = abilityDatabase[abilityIndex];
        if (castingVisualEffect != null) //Make sure we destoy active IF effect is still active.
        {
            Destroy(castingVisualEffect.gameObject);
            castingVisualEffect = null;
        }

        if (castingVisualEffect == null)
        {
            GameObject newActivatonVFX = Instantiate(ability.castingVisualEffect.gameObject, transform.position + transform.forward * 0.25f + transform.up * 0.35f, Quaternion.Euler(transform.rotation.eulerAngles));
            newActivatonVFX.transform.parent = transform;
            newActivatonVFX.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles);

            castingVisualEffect = newActivatonVFX.transform;
        }
    }

    [ObserversRpc(RunLocally = true)]
    void ClientRPC_CreateActivationEffect(int abilityIndex)
    {
        var ability = abilityDatabase[abilityIndex];
        GameObject newActivatonVFX = Instantiate(ability.activateVFX.gameObject, playerController.transform.position + transform.TransformDirection(ability.activateVFXOffset), Quaternion.Euler(playerController.transform.rotation.eulerAngles));
        newActivatonVFX.transform.SetParent(playerController.transform, true);
        switch (ability.activeVFXLifeTime)
        {
            case Ability.VFXLifeTime.ActiveTimeOffset:
                Destroy(newActivatonVFX, ability.activeTime + ability.activeVFXlifeTimeValue);
                break;
            case Ability.VFXLifeTime.DestroyAfter:
                Destroy(newActivatonVFX, ability.activeVFXlifeTimeValue);
                break;
            default:
                break;
        }
    }

    private IEnumerator PerformAbility()
    {
        float castTimer = 0f;
        int abilityIndex = selectedAbilityIndex;
        if (activatedAbility.ability.castingVisualEffect != null)
        {
            ServerRPC_CreateCastingVisual(abilityIndex);
        }

        while (castTimer < activatedAbility.ability.castTime)
        {
            activatedAbility.state = AbilityHolder.abillityState.casting;

            castTimer += Time.deltaTime;

            playerController.playerHUD.updateActionBar(activatedAbility.ability.abilityName, activatedAbility.ability.castTime, castTimer, activatedAbility.ability.icon);

            yield return new WaitForEndOfFrame();
        }


        activatedAbility.Activate();
        TriggerAbilityEffect(abilityIndex);

        yield break;
    }



    [ServerRpc(RunLocally = false)]
    private void TriggerAbilityEffect(int abilityIndex)
    {
        ClientRPC_RemoveCastingEffect();

        var ability = abilityDatabase[abilityIndex];

        ability.ActivateServer(playerController);
        if (ability.activateVFX)
        {
            ClientRPC_CreateActivationEffect(abilityIndex);
        }
    }

    [ObserversRpc]
    void ClientRPC_RemoveCastingEffect()
    {
        if (castingVisualEffect != null)
        {
            Destroy(castingVisualEffect.gameObject);
            castingVisualEffect = null;
        }
    }

    void Update()
    {
        for (int i = 0; i < abilityHolder.Count; i++)
        {
            AbilityHolder abilityH = abilityHolder[i];

            switch (abilityH.state)
            {
                case AbilityHolder.abillityState.ready:
                    break;
                case AbilityHolder.abillityState.casting:
                    break;
                case AbilityHolder.abillityState.active:
                    if (abilityH.activeTime > 0)
                    {
                        abilityH.activeTime -= Time.deltaTime;

                        if (abilityH.activeIntervalTimer > 0) 
                        {
                            abilityH.activeIntervalTimer -= Time.deltaTime;
                        }
                        else
                        {
                            abilityH.activeIntervalTimer = abilityH.ability.activeInterval;
                            abilityH.ability.ContiniousEffect(playerController, abilityH);
                        }

                        if (abilityH.buffDeBuffElement == null)
                        {
                            abilityH.buffDeBuffElement = playerController.playerHUD.CreateBuffDeBuffElement();
                            abilityH.buffDeBuffElement.buffDebuffIcon.sprite = abilityH.ability.icon;
                        }
                        else
                        {
                            abilityH.buffDeBuffElement.UpdateBuffDebuffDuration(abilityH.activeTime);
                        }
                    }
                    else
                    {
                        abilityH.state = AbilityHolder.abillityState.cooldown;
                        abilityH.cooldownTime = abilityH.ability.cooldownTime;

                        if (abilityH.buffDeBuffElement != null)
                        {
                            Destroy(abilityH.buffDeBuffElement.gameObject);
                        }

                        abilityH.ability.OnContiniousEffectEnd(playerController);
                    }
                    break;
                case AbilityHolder.abillityState.cooldown:
                    if (abilityH.cooldownTime > 0)
                    {
                        abilityH.cooldownTime -= Time.deltaTime;
                    }
                    else
                    {
                        abilityH.state = AbilityHolder.abillityState.ready;
                    }
                    break;
                default:
                    break;
            }
        }
    }

    internal void SetupAbilities()
    {
        if (IsOwner)
        {
            List<int> abilityIndecies = new List<int>(abillities.Count);
            for (int i = 0; i < abillities.Count; i++)
            {
                int abilityIndex = abilityDatabase.FindIndex((a) => a == abillities[i]);

                try
                {
                    if (abilityIndex == -1)
                    {
                        throw new Exception($"Tried to select {abillities[i].abilityName}, it is not addressable/have the ability lable as an Ability.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                    continue;
                }

                abilityIndecies.Add(abilityIndex);

                AbilityHolder abillityState = new AbilityHolder
                {
                    ability = abillities[i],
                    cooldownTime = abillities[i].cooldownTime,
                    activeTime = abillities[i].activeTime
                };

                abilityHolder.Add(abillityState);

                playerController.playerHUD.abilityButtons[i].gameObject.SetActive(true);
                playerController.playerHUD.abilityButtons[i].abilityHolder = abillityState;
                playerController.playerHUD.abilityButtons[i].abilityImage.sprite = abillityState.ability.icon;
            }
            SelectAbillity(0);
            SyncAbilities(abilityIndecies);
        }
    }

    [ServerRpc]
    public void PlayCastSoundServer(int abilityIndex)
    {
        PlayCastSoundClient(abilityIndex);
    }

    [ObserversRpc(ExcludeOwner = true)]
    public void PlayCastSoundClient(int abilityIndex)
    {
        Ability ability = Ability.abilityDatabase[abilityIndex];
        audioSource.PlayOneShot(ability.castSFX != null ? ability.castSFX : genericAbilityCastSFX);
    }
}
