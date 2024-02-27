using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum HumanoidSound
{
    Hurt,
    Dying,
}

[Serializable]
public class SoundCollection
{
    [HideInInspector]
    public string name = "T";
    public AudioClip[] sounds;
}

public class HumanoidSoundPlayer : MonoBehaviourRefSetup
{

    static SoundCollection[] GetDefaultSoundCollection()
    {
        SoundCollection[] soundCollection = new SoundCollection[Enum.GetValues(typeof(HumanoidSound)).Length];
        for (int i = 0; i < soundCollection.Length; i++)
        {
            soundCollection[i] = new();
            soundCollection[i].name = ((HumanoidSound)i).ToString();
        }
        return soundCollection;
    }
    public SoundCollection[] humanoidSounds = GetDefaultSoundCollection();

    [SerializeField, HideInInspector] AudioSource audioSource;
    override protected void SetupReferences()
    {
        audioSource = GetComponent<AudioSource>();
    }

    [ContextMenu("Update Sound Collection Array")]
    void UpdateSoundCollectionArray()
    {
        var newSoundCollection = GetDefaultSoundCollection();
        for (int i = 0; i < this.humanoidSounds.Length; i++)
        {
            int soundCount = humanoidSounds[i].sounds.Length;
            newSoundCollection[i].sounds = new AudioClip[soundCount];
            for (int soundIndex = 0; soundIndex < soundCount; soundIndex++)
            {
                newSoundCollection[i].sounds[soundIndex] = humanoidSounds[i].sounds[soundIndex];
            }
        }
        humanoidSounds = newSoundCollection;
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
    }

    public void Start()
    {

        var healthComp = GetComponent<Health>();
        healthComp.OnHit += OnHit;
        healthComp.OnDeath += OnDeath;
    }

    void OnHit(Health health, WeaponTypeData weaponData, bool parried)
    {
        if(parried)
        {
            audioSource.PlayOneShot(weaponData.GetRandomWeaponSound(WeaponSounds.Parry));
        }
        else
        {
            audioSource.PlayOneShot(weaponData.GetRandomWeaponSound(WeaponSounds.MeatHit));

            StartCoroutine(PlaySoundAtDelay(0.2f, HumanoidSound.Hurt));
        }

    }

    IEnumerator PlaySoundAtDelay(float time, HumanoidSound soundType)
    {
        yield return new WaitForSeconds(time);
        PlayRandomSound(soundType);

    }

    void OnDeath(Health health)
    {
        PlayRandomSound(HumanoidSound.Dying);
    }

    void PlayDefaultSound(HumanoidSound sound)
    {
        var soundCollection = humanoidSounds[(int)sound].sounds;
        int nrOfSounds = soundCollection.Length;
        if (nrOfSounds > 0)
        {
            audioSource.PlayOneShot(soundCollection[0]);
        }
    }

    void PlayRandomSound(HumanoidSound sound)
    {
        var soundCollection = humanoidSounds[(int)sound].sounds;
        int nrOfSounds = soundCollection.Length;
        if (nrOfSounds > 0)
        {
            int soundIndex = UnityEngine.Random.Range(0, nrOfSounds - 1);
            audioSource.PlayOneShot(soundCollection[soundIndex]);
        }
    }
}
