using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
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

    public void Update()
    {
        if (!IsOwner)
        {
            return;
        }

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
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }

        if (hasTriggeredImpact)
        {
            return;
        }

        if (Physics.Linecast(transform.position, transform.position + transform.forward * raycastLengthModifier, out RaycastHit hitInfo, collisionLayer))
        {
            hasTriggeredImpact = true;

            if (hitInfo.transform.GetComponent<Health>())
            {
                hitInfo.transform.GetComponent<Health>().TakeDamage(damage, 0, transform);
            }
            AudioManager.instance.PlaySound(transform.position, hitSound);

            if (stopMovementOnImpact)
            {
                allowMove = false;
            }

            /// VFX
            if (impactVFX != null)
            {
                Transform vfx = Instantiate(impactVFX, transform.position, impactVFX.rotation);
                Destroy(vfx.gameObject, 5f);
            }
        }
    }

    private void OnDestroy()
    {
        if (IsOwner)
        {
            return;
        }

        AudioManager.instance.PlaySound(transform.position, hitSound);

        /// VFX
        if (impactVFX != null)
        {
            Transform vfx = Instantiate(impactVFX, transform.position, impactVFX.rotation);
            Destroy(vfx.gameObject, 5f);
        }
    }
}
