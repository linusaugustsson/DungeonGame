using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class FollowTransform : NetworkBehaviour
{
    public Transform followTarget;
    public float followSpeed = 2.5f;
    public bool ignoreY = true;

    [SyncVar]public SimplePlayerController playerController;

    private Vector3 targetPos;

    void Update()
    {
        if (followTarget != null)
        {
            if (playerController != null)
            {
                followSpeed = playerController.transitionSpeed;
            }

            targetPos = ignoreY ? new Vector3(followTarget.position.x, transform.position.y, followTarget.position.z) : followTarget.position;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * followSpeed);
        }
    }
}
