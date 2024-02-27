using FishNet;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireballScript : NetworkBehaviour
{

    public float speed = 1.0f;
    public float directDamage = 60.0f;
    public float splashDamage = 30.0f;

    public LayerMask collisionLayer;

    public AudioClip fireballHitSound;
    [Space]

    public Transform fireballExplosionVFXPrefab;

    public void Update()
    {

        if(!IsOwner)
        {
            return;
        }

        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        


        if(Physics.Linecast(transform.position, transform.position + transform.forward * 0.1f, out RaycastHit hitInfo, collisionLayer))
        {
            if(hitInfo.transform.GetComponent<Health>())
            {
                hitInfo.transform.GetComponent<Health>().TakeDamage(directDamage, 0, transform);
            }
            AudioManager.instance.PlaySound(transform.position, fireballHitSound);

            /// VFX
            if (fireballExplosionVFXPrefab != null)
            {
                Transform fireballExplosionVFX = Instantiate(fireballExplosionVFXPrefab, transform.position, fireballExplosionVFXPrefab.rotation);
                Destroy(fireballExplosionVFX.gameObject, 5f);
            }

            Destroy(gameObject);
        }


    }

    [ServerRpc]
    public void SpawnFireballVFX()
    {
        /// VFX
        if (fireballExplosionVFXPrefab != null)
        {
            Transform fireballExplosionVFX = Instantiate(fireballExplosionVFXPrefab, transform.position, fireballExplosionVFXPrefab.rotation);

            /// VFX will destroy itself
            InstanceFinder.ServerManager.Spawn(fireballExplosionVFX.gameObject, LocalConnection);
        }
    }


    private void OnDestroy()
    {
        if (IsOwner)
        {
            return;
        }
        AudioManager.instance.PlaySound(transform.position, fireballHitSound);

        /// VFX
        if (fireballExplosionVFXPrefab != null)
        {
            Transform fireballExplosionVFX = Instantiate(fireballExplosionVFXPrefab, transform.position, fireballExplosionVFXPrefab.rotation);
        }
    }

    

}
