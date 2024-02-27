using System.Collections.Generic;
using UnityEngine;

public enum WeaponSounds
{
    MeatHit,
    Parry,
    Swing,
    Draw,
    Sheath,
}

[CreateAssetMenu(fileName = "WeaponTypeData", menuName = "Weapon/Weapon Type", order = 1)]
public class WeaponTypeData : ScriptableObject
{
    public static List<WeaponTypeData> allWeaponTypes;
    public AudioClip[] meatHitSounds;
    public AudioClip[] parrySounds;
    public AudioClip[] swingSounds;
    public AudioClip[] drawSound;
    public AudioClip[] sheathSound;

    private void OnEnable()
    {
        
    }

    public AudioClip GetRandomWeaponSound(WeaponSounds weaponSound)
    {
        AudioClip[] audioClips = null;
        switch (weaponSound)
        {
            case WeaponSounds.MeatHit:
                audioClips = meatHitSounds;
                break;
            case WeaponSounds.Parry:
                audioClips = parrySounds;
                break;
            case WeaponSounds.Swing:
                audioClips = swingSounds;
                break;
            case WeaponSounds.Draw:
                audioClips = drawSound;
                break;
            case WeaponSounds.Sheath:
                audioClips = sheathSound;
                break;
            default:
                break;
        }
        if(audioClips.Length == 0)
        {
            return default;
        }
        else
        {
            int audioIndex = Random.Range(0, audioClips.Length - 1);
            return audioClips[audioIndex];
        }

    }
}
