using FishNet.Example.Scened;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteLook : MonoBehaviour
{
    private Transform _targetTransform;

    private bool _catchPlayer = true;

    private SimplePlayerController _playerController;
    public SpriteRenderer targetSpriteRenderer;

    private void Awake()
    {
        if(targetSpriteRenderer == null)
        {
            targetSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        GetTargetPlayer();

        if (_targetTransform == null)
        {
            return;
        }

        Vector3 modifiedTarget = _targetTransform.position;
        modifiedTarget.y = targetSpriteRenderer.transform.position.y;
        targetSpriteRenderer.transform.LookAt(modifiedTarget);
        targetSpriteRenderer.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));
    }

    private void GetTargetPlayer()
    {
        if (_catchPlayer)
        {
            if (SteamLobby.instance.sessionManager != null)
            {
                if (SteamLobby.instance.sessionManager.myCharacter != null)
                {
                    _targetTransform = SteamLobby.instance.sessionManager.myCharacter.transform;
                    _catchPlayer = false;
                    _playerController = GetComponent<SimplePlayerController>();
                }
            }
        }
    }

}
