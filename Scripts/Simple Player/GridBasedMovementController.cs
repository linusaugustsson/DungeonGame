using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using FishNet.Object.Synchronizing;
using UnityEngine.UI;
using FishNet;
using UnityEngine.Events;
using System;

public class SimplePlayerController : NetworkBehaviour
{
    #region Movement
    [Header("MOVEMENT")]
    public bool smoothTransition = false;
    public bool noClip = false;
    public float transitionSpeed = 10f;
    public float transitionRotationSpeed = 500f;

    [SerializeField]private Level currentLevel; //Can be here but maybe something could be better.
    [SerializeField] private Vector3Int currentTilePosition;
    [SerializeField] private Vector3 targetGridPos;
    [SerializeField] private Vector3 targetRotation;

    public Action OnMovementStarted;
    public Action OnMovementEnded;
    public Action<NetworkPlayerData> OnClientInitalized;
    public NetworkPlayerData networkPlayerData;
    public bool clientInitalized = false;

    //--Move to other scripts--

    

    //Move to input script
    private AbillityUser abilityUser;

    //-Action-  Would ideally be an equiped item that activated on assigned inventory hotkey.
    public GameObject pointLight;
    [SyncVar(OnChange = nameof(OnLightChange))] public bool lightOn = false;





    private void Awake()
    {
        abilityUser = GetComponent<AbillityUser>();

    }

    private void OnRejectedTileMovement(Vector3Int returnTile)
    {
        currentTilePosition = returnTile;
        targetGridPos = currentLevel.TileToWorldPos(returnTile);
    }

    public void OnLightChange(bool prev, bool next, bool asServer)
    {
        pointLight.SetActive(next);
    }

    public void RotateLeft() { if (AtRest(0.5f)) targetRotation -= Vector3.up * 90f; }
    public void RotateRight() { if (AtRest(0.5f)) targetRotation += Vector3.up * 90f; }

    public void Move(Vector3 direction)
    {
        if (!AtRest()) return;
        Vector3Int destination = currentTilePosition + Vector3Int.RoundToInt(transform.TransformDirection(direction));
        //Debug.Log("can move to tile: " + currentLevel.CanMoveToTile(destination));
        if (noClip || currentLevel.CanMoveToTile(destination))
        {
            OnMove();
            currentLevel.ModifyOccupancy(currentTilePosition, destination);
            currentTilePosition = destination;
            targetGridPos = currentLevel.TileToWorldPos(destination);
            PlayFootstepSFX();
            OnMovementStarted?.Invoke();
            ServerTriggerMovement(true);
        }
    }

    bool AtRest(float distance = 0.05f)
    {
        return Vector3.Distance(transform.position, targetGridPos) < distance && Vector3.Distance(transform.eulerAngles, targetRotation) < distance;
    }


    public void OnMove()
    {
        abilityUser.InterruptAbilityCastOnMove();
    }

    private void FixedUpdate()
    {
        if (MenuManager.instance != null && base.IsOwner == false)
        {
            MenuManager.instance.CloseAllMenus();
            return;
        }

        if (AtRest())
        {
            OnMovementEnded?.Invoke();
            ServerTriggerMovement(false);
        }

        MovePlayer();
    }

    public void MovePlayer()
    {
        /// ROTATION
        if (targetRotation.y > 270f && targetRotation.y < 361f) targetRotation.y = 0f;
        if (targetRotation.y < 0f) targetRotation.y = 270f;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * transitionRotationSpeed);

        Vector3 targetPosition = targetGridPos;

        if (GetComponent<AIController>() != null) return;
        /// MOVEMENT
        if (!smoothTransition)
        {
            transform.SetPositionAndRotation(targetPosition, Quaternion.Euler(targetRotation));
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
        }
    }

    public void ForcePosition(Vector3 position)
    {
        var oldPos = currentTilePosition;
        currentTilePosition = currentLevel.PosToTile(position, out targetGridPos);
        currentLevel.ModifyOccupancy(oldPos, currentTilePosition, true);
        transform.position = targetGridPos;
    }

    #endregion


    [Header("HUD")]
    public PlayerHUD playerHUD;


    #region SFX
    public AudioSource audioSoruce;
    public AudioClip[] footstepSFX;

    public AnimationCurve footstepVolumeCurve;

    private AudioClip previousSFXSound;


    public void PlayFootstepSFX()
    {
        int index = UnityEngine.Random.Range(1, 4);

        if (index > 1)
        {
            //audioSoruce.PlayOneShot(footstepSFX[0]);
            previousSFXSound = footstepSFX[0];
            AudioManager.instance.PlaySoundFootstep(transform.position, footstepSFX[0], transform);
        }
        else if (previousSFXSound != footstepSFX[footstepSFX.Length -1])
        {
            //audioSoruce.PlayOneShot(footstepSFX[footstepSFX.Length -1 ]);
            previousSFXSound = footstepSFX[footstepSFX.Length - 1];
            AudioManager.instance.PlaySoundFootstep(transform.position, footstepSFX[footstepSFX.Length - 1], transform);
        }
        else
        {
            //audioSoruce.PlayOneShot(footstepSFX[0]);
            previousSFXSound = footstepSFX[0];
            AudioManager.instance.PlaySoundFootstep(transform.position, footstepSFX[0], transform);
        }

        PlayFootstepNetwork(index);
    }
    #endregion


    #region Network



    public override void OnStartClient()
    {
        base.OnStartClient();

        //REFACTOR
        if(playerHUD != null)
            playerHUD.gameObject.SetActive(IsOwner);

        currentLevel = SteamLobby.instance.sessionManager.spawnedLevel;
        currentLevel.OnRejectedTileMovement += OnRejectedTileMovement;
        currentTilePosition = currentLevel.PosToTile(transform.position, out targetGridPos);


        
    }



    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);
        NetworkPlayerData[] networkPlayerDatas = FindObjectsOfType<NetworkPlayerData>();

        if (IsOwner == false)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);

            //gameObject.layer = LayerMask.NameToLayer("Obstacle");
        }
        else
        {
            MenuManager.instance.CloseAllMenus();
            GetComponentInChildren<Camera>().gameObject.SetActive(true);
            SteamLobby.instance.sessionManager.myCharacter = gameObject;

            foreach (var plyData in networkPlayerDatas)
            {
                if (plyData.IsOwner)
                {
                    OnClientInitalized?.Invoke(plyData);
                    networkPlayerData = plyData;
                    clientInitalized = true;
                    transform.name = Class.classDatabase[plyData.selectedClass].className;
                    if (abilityUser != null)
                    {
                        if (Ability.abilityDatabase == null)
                        {
                            AddressableHandler.OnAddressablesLoaded += () => { abilityUser.abillities = plyData.selectedAbilityIndexes.ConvertAll<Ability>((abilityIndex) => Ability.abilityDatabase[abilityIndex]); };
                        }
                        else
                        {
                            abilityUser.abillities = plyData.selectedAbilityIndexes.ConvertAll<Ability>((abilityIndex) => Ability.abilityDatabase[abilityIndex]);
                        }
                        abilityUser.SetupAbilities();
                    }
                }
            }

        }


    }

    [ServerRpc]
    public void ToggleLight()
    {
        if(lightOn == true)
        {
            lightOn = false;
        } else
        {
            lightOn = true;
        }
    }
    [ServerRpc]
    private void PlayFootstepNetwork(int index)
    {
        PlayFootstepNetworkClient(index);
    }

    [ObserversRpc]
    private void PlayFootstepNetworkClient(int index)
    {
        if (IsOwner == true)
        {
            return;
        }
        if (index > 1)
        {
            //audioSoruce.PlayOneShot(footstepSFX[0]);
            previousSFXSound = footstepSFX[0];
            AudioManager.instance.PlaySoundFootstep(transform.position, footstepSFX[0], transform);
        }
        else if (previousSFXSound != footstepSFX[footstepSFX.Length - 1])
        {
            //audioSoruce.PlayOneShot(footstepSFX[footstepSFX.Length - 1]);
            previousSFXSound = footstepSFX[footstepSFX.Length - 1];
            AudioManager.instance.PlaySoundFootstep(transform.position, footstepSFX[footstepSFX.Length - 1], transform);
        }
        else
        {
            //audioSoruce.PlayOneShot(footstepSFX[0]);
            previousSFXSound = footstepSFX[0];
            AudioManager.instance.PlaySoundFootstep(transform.position, footstepSFX[0], transform);

        }
    }


    [ServerRpc(RunLocally = false)]
    public void ServerTriggerMovement(bool isMoving)
    {
        ClientTriggerAnimation(isMoving);
    }

    [ObserversRpc(ExcludeOwner = true)]
    private void ClientTriggerAnimation(bool isMoving)
    {
        if (isMoving)
            OnMovementStarted?.Invoke();
        else
            OnMovementEnded?.Invoke();
    }

    [ObserversRpc]
    public void SetClientPosition(Vector3 position)
    {
        if(IsOwner)
        {
            ForcePosition(position);
        }
    }

    #endregion

}
