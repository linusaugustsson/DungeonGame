using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapon/Weapon", order = 1)]
public class WeaponData : ScriptableObject
{
    [Header("Attack Data")]
    public WeaponTypeData weaponType;
    public int width, reach;
    public float baseDamage;
    public float attackDuration;
    public float cooldown;
    [Header("Parry")]
    public float parryDuration;
    public float parryTransitionTime;
    public float parryCooldown;
    [Header("View Model Settings")]
    public Sprite viewModelSprite;
    public Vector3 parryRotation;
    public Vector3 parryPosition;
    [Header("World Model Settings")]
    public Sprite worldModelSprite;
}
