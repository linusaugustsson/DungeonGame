using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Class", menuName = "Create Class", order = 1)]
public class Class : ScriptableObject, IComparable<Class>
{
    static public List<Class> classDatabase = new();
    public string className;
    public string description;
    public Sprite classViewSprite;

    public WeaponData defaultWeapon;
    public int nrOfAbilitySlots = 3;
    public List<Ability> avalibleClassAbilities;

    public int CompareTo(Class obj)
    {
        return this.className.CompareTo(obj.className);
    }
}
