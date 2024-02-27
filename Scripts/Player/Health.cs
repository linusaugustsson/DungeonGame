using System;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections;
using UnityEngine.Events;

public class Health : NetworkBehaviour
{
    public static Action<Health> OnAnyDeath;
    public float maxHealth = 100;
    [SyncVar(OnChange = nameof(OnHealthChange))]  private float healthAmount;
    [SyncVar(OnChange = nameof(OnAliveChange))] public bool isAlive;

    public Action<Health> OnDeath;
    public Action<Health, WeaponTypeData, bool> OnHit;
    public Action<Health> OnAlive;

    private HitDirectionalUI hitDirectionalUI;
    private SimplePlayerController playerController;
    private AbillityUser abilityUser;

    [Range(-1, 1)][SyncVar]
    public float damageResistance = 0f;

    public UnityEvent onDamageTaken;

    public GameObject playerCorpsePrefab;
    private GameObject spawnedCorpse;

    void OnAliveChange(bool prev, bool next, bool asServer)
    {
        if (!asServer && !prev && isAlive)
        {
            OnAlive?.Invoke(this);
            if(gameObject.IsPlayer() && IsOwner)
                PostProcessingSettings.instance.SetAliveLook();
        }
    }

    private void Awake()
    {
        abilityUser = GetComponent<AbillityUser>();
        hitDirectionalUI = GetComponent<HitDirectionalUI>();
        playerController = GetComponent<SimplePlayerController>();
    }

    private void Start()
    {
        InitialState();


        //REFACTOR
        /// Update health bar
        if (playerController != null && playerController.playerHUD != null) //Break this connection by subscribing to OnHit or similar in PlayerHud Instead.
        {
            playerController.playerHUD.updateHealthBar(maxHealth, healthAmount);
        }
    }

    [ServerRpc]
    internal void SetDamageResistance(float newDamageResistance)
    {
        damageResistance = newDamageResistance;
    }

    private void OnHealthChange(float prev, float next, bool asServer)
    {
        if (healthAmount <= 0f)
        {
            if (asServer)
            {
                isAlive = false;
                //StartCoroutine(ResetHealthAfterTime(1f));
            }
            else
            {
                OnDeath?.Invoke(this);
                OnAnyDeath?.Invoke(this);

                //REFACTOR
                spawnedCorpse = Instantiate(playerCorpsePrefab, transform.position, Quaternion.identity);
            }
        }
        //REFACTOR
        if (IsOwner && !asServer && playerController != null && playerController.playerHUD != null)
        {
            playerController.playerHUD.updateHealthBar(maxHealth, healthAmount);
        }
    }

    IEnumerator ResetHealthAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        InitialState();
    }


    [ServerRpc(RunLocally = false, RequireOwnership = false)]
    public void TakeDamage(float damageTaken, int weaponTypeID, Transform sender)
    {
        
        int parryModifier = Convert.ToInt32(!abilityUser.parrying);
        damageTaken = (damageTaken - damageTaken * damageResistance) * parryModifier;

        healthAmount = Mathf.Min(maxHealth, healthAmount - damageTaken);
        ClientTakeDamage(weaponTypeID, sender, abilityUser.parrying);

        //// CHANGE LATER WHEN WE HAVE STATUS EFFECTS
        //damageResistance = 0f;

        onDamageTaken?.Invoke();
    }

    [ServerRpc(RunLocally = false, RequireOwnership = false)]
    public void RestoreHealth(float healing, Transform sender)
    {
        healthAmount = Mathf.Min(maxHealth, healthAmount + healing);
    }

    [ObserversRpc]
    private void ClientTakeDamage(int weaponTypeID, Transform sender, bool parried)
    {
        WeaponTypeData weaponType = WeaponManager.instance.weaponTypes[weaponTypeID];
        OnHit?.Invoke(this, weaponType, parried);

        if (IsOwner &&  hitDirectionalUI != null && sender != null)
        {
            hitDirectionalUI.triggerHitIndicator(sender.position);
        }
    }

    public void InitialState()
    {
        healthAmount = maxHealth;
        isAlive = true;
    }

    //REFACTOR
    public void ClearCorpse()
    {
        Destroy(spawnedCorpse);
    }
}
