using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burst : NetworkBehaviour
{
    [Header("Burst Settings")]
    public float hitDelay = 0f;
    public float hitRadius;
    public LayerMask targetLayer;
    [Space]
    public bool destroyOnHit = true;
    public float destroyAfter = 0f;
    [Header("Continious Burst Settings")]
    public float burstInterval = 0f;
    public float burstduration = 0f;
    public bool destroyAfterDurationFinished = true;

    [Header("Damage settings")]
    public float damage = 1f;

    internal Transform sender;

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(BurstLogic());
    }

    IEnumerator BurstLogic()
    {
        yield return new WaitForSeconds(hitDelay);

        if (burstInterval <= 0)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius, targetLayer);

            if (hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    DamageTarget(hits[i].gameObject);
                }
            }

            yield return new WaitForEndOfFrame();

            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
            else
            {
                yield return new WaitForSeconds(destroyAfter);
                Destroy(gameObject);
            }
        }
        else
        {
            float timer = 0f;
            while (timer < burstduration)
            {
                timer += Time.deltaTime;

                Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius, targetLayer);

                if (hits.Length > 0)
                {
                    for (int i = 0; i < hits.Length; i++)
                    {
                        DamageTarget(hits[i].gameObject);

                        if (destroyOnHit)
                        {
                            Destroy(gameObject);
                        }
                    }
                }

                if (timer >= destroyAfter)
                {
                    Destroy(gameObject);
                }

                yield return new WaitForSeconds(burstInterval);
            }

            if (destroyAfterDurationFinished)
            {
                Destroy(gameObject);
            }
        }

    }

    public void DamageTarget(GameObject target)
    {
        Health _health = target.GetComponent<Health>();
        if (_health != null)
        {
            _health.TakeDamage(damage, 0, transform);
        }
    }
}
