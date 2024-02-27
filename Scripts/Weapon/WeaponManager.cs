using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    static public WeaponManager instance;
    public WeaponTypeData[] weaponTypes;
    private void Awake()
    {
        if(instance != null)
        {
            Destroy(transform);
            return;
        }
        instance = this;
    }



}
