using FishNet.Example.Scened;
using FishNet.Object;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAngle : NetworkBehaviour
{

    private Transform _targetTransform;

    public int lastIndex = 0;

    private bool _catchPlayer = true;

    private Vector3 _targetPos;
    private Vector3 _targetDir;

    private float _angle;

    public SpriteRenderer targetSpriteRenderer;


    public Sprite backSprite;
    public Sprite forwardSprite;
    public Sprite sideLeftSprite;
    public Sprite sideRightSprite;

    private SimplePlayerController _playerController;
    public Action<float> OnAngleChange;


    private void Update()
    {
        GetTargetPlayer();

        if(_targetTransform == null)
        {
            return;
        }


        // Billboard
        Vector3 modifiedTarget = _targetTransform.position;
        modifiedTarget.y = targetSpriteRenderer.transform.position.y;
        targetSpriteRenderer.transform.LookAt(modifiedTarget);
        targetSpriteRenderer.transform.Rotate(new Vector3(0.0f, 180.0f, 0.0f));




        // Get Target Position and Direction
        _targetPos = new Vector3(_targetTransform.position.x, transform.position.y, _targetTransform.position.z);
        _targetDir = _targetPos - transform.position;

        _angle = Vector3.SignedAngle(_targetDir, transform.forward, Vector3.up);

        //Flip Sprite if needed
        /*
        Vector3 tempScale = Vector3.one;
        if (_angle > 0) { tempScale.x *= -1f; }
        targetSpriteRenderer.transform.localScale = tempScale;
        */


        lastIndex = GetIndex(_angle);

        OnAngleChange?.Invoke((float)lastIndex);
    }

    private int GetIndex(float angle)
    {
        /*
        //front
        if (angle > -22.5f && angle < 22.6f)
            return 0;
        if (angle >= 22.5f && angle < 67.5f)
            return 7;
        if (angle >= 67.5f && angle < 112.5f)
            return 6;
        if (angle >= 112.5f && angle < 157.5f)
            return 5;


        //back
        if (angle <= -157.5 || angle >= 157.5f)
            return 4;
        if (angle >= -157.4f && angle < -112.5f)
            return 3;
        if (angle >= -112.5f && angle < -67.5f)
            return 2;
        if (angle >= -67.5f && angle <= -22.5f)
            return 1;
        */


        //front
        if (angle > -45.0f && angle < 45.0f)
        {
            //Debug.Log("Forward");
            return 0;
        }
        if (angle >= 45.0f && angle < 135.0f)
        {
            //Debug.Log("SideRight"); // reversed
            return 1;
        }
            

        //back
        if (angle <= -135.0f || angle >= 135.0f)
        {
            //Debug.Log("Back");
            return 2;
        }
        if (angle >= -135.0f && angle <= -45.5f)
        {
            //Debug.Log("SideLeft"); // reversed
            return 3;
        }
            

        return lastIndex;
    }

    private void GetTargetPlayer()
    {
        if(_catchPlayer)
        {
            if(SteamLobby.instance.sessionManager != null)
            {
                if(SteamLobby.instance.sessionManager.myCharacter != null)
                {
                    _targetTransform = SteamLobby.instance.sessionManager.myCharacter.transform;
                    _catchPlayer = false;
                    _playerController = GetComponent<SimplePlayerController>();
                }
            }
        }
    }

    public void SetSpriteTest()
    {
        switch (lastIndex)
        {
            case 0:
                //Debug.Log("set sprite forward");
                targetSpriteRenderer.sprite = forwardSprite;
                break;
            case 1:
                //Debug.Log("set sprite side right");
                targetSpriteRenderer.sprite = sideRightSprite;
                break;
            case 2:
                //Debug.Log("set sprite back");
                targetSpriteRenderer.sprite = backSprite;
                break;
            case 3:
                //Debug.Log("set sprite sideleft");
                targetSpriteRenderer.sprite = sideLeftSprite;
                break;
            default:
                //Debug.Log("set sprite forward");
                targetSpriteRenderer.sprite = forwardSprite;
                break;
        }
    }


}
