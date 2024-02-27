using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackVolume : MonoBehaviourRefSetup
{
    internal WeaponData weaponData;
    internal Transform sender;

    [SerializeField, HideInInspector] private Collider attackVolumeCollider;

    private void Start()
    {
        StartCoroutine(RemoveAfterTime(weaponData ? weaponData.attackDuration : 0f));
    }

    IEnumerator RemoveAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform != sender)
        {
            int weaponTypeID = Array.FindIndex(WeaponManager.instance.weaponTypes, w => w.Equals(weaponData.weaponType));
            other.GetComponent<Health>().TakeDamage(weaponData.baseDamage, weaponTypeID, sender);
            Destroy(gameObject);
        }
    }

    override protected void SetupReferences()
    {
        attackVolumeCollider = GetComponent<Collider>();
    }
}
