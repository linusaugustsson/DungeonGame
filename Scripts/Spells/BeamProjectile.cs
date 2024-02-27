using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamProjectile : NetworkBehaviour
{
    [Header("Beam Settings")]
    public LineRenderer lineRenderer;
    public Transform endPoint;
    public bool hitScan = false;

    [Header("Projectile Settings")]
    public float speed = 1.0f;
    public float damage = 25.0f;
    public bool destroyOnImpact = true;
    public float destroyAfter = 0f;
    public bool stopMovementOnImpact = false;
    [Space]
    public float raycastLengthModifier = 0.1f;

    [Header("Layers")]
    public LayerMask collisionLayer;

    [Header("SFX")]
    public AudioClip hitSound;
    [Space]

    [Header("VFX")]
    public Transform impactVFX;

    internal bool allowMove = true;

    private bool hasTriggeredImpact = false;


    void Start()
    {
        if (hitScan)
        {
            allowMove = false;
        }

        lineRenderer.SetPosition(0, transform.position);
    }

    public void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        lineRenderer.SetPosition(1, endPoint.position);

        if (hasTriggeredImpact)
        {
            if (destroyOnImpact)
            {
                Destroy(gameObject);
            }
            else if (destroyAfter > 0)
            {
                destroyAfter -= Time.deltaTime;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        if (allowMove)
        {
            endPoint.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        if (hasTriggeredImpact)
        {
            return;
        }

        if (hitScan)
        {
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hitInfo, Mathf.Infinity, collisionLayer))
            {
                hasTriggeredImpact = true;

                endPoint.transform.position = hitInfo.point;

                if (hitInfo.transform.GetComponent<Health>())
                {
                    hitInfo.transform.GetComponent<Health>().TakeDamage(damage, 0, transform);
                }

                AudioManager.instance.PlaySound(endPoint.position, hitSound);

                /// VFX
                if (impactVFX != null)
                {
                    Transform vfx = Instantiate(impactVFX, endPoint.position, impactVFX.rotation);
                }
            }
        }
        else
        {
            if (Physics.Linecast(endPoint.position, endPoint.position + endPoint.forward * raycastLengthModifier, out RaycastHit hitInfo, collisionLayer))
            {
                hasTriggeredImpact = true;

                if (hitInfo.transform.GetComponent<Health>())
                {
                    hitInfo.transform.GetComponent<Health>().TakeDamage(damage, 0, transform);
                }
                AudioManager.instance.PlaySound(endPoint.position, hitSound);

                if (stopMovementOnImpact)
                {
                    allowMove = false;
                }

                /// VFX
                if (impactVFX != null)
                {
                    Transform vfx = Instantiate(impactVFX, endPoint.position, impactVFX.rotation);
                    Destroy(vfx.gameObject, 5f);
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (IsOwner)
        {
            return;
        }

        AudioManager.instance.PlaySound(endPoint.position, hitSound);

        /// VFX
        if (impactVFX != null)
        {
            Transform vfx = Instantiate(impactVFX, endPoint.position, impactVFX.rotation);
            Destroy(vfx.gameObject, 5f);
        }
    }
}
