using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public GameObject soundPrefab;

    public GameObject footstepPrefab;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(this);


    }

    public void PlaySound(Vector3 position, AudioClip sound, AnimationCurve customCurve = null)
    {
        GameObject newSound = Instantiate(soundPrefab, position, Quaternion.identity);
        AudioSource audioSource = newSound.GetComponent<AudioSource>();
        audioSource.PlayOneShot(sound);
    }

    public void PlaySoundRandomPitch(Vector3 position, AudioClip sound, float minPitch, float maxPitch, AnimationCurve customCurve = null)
    {
        GameObject soundObject = Instantiate(soundPrefab, position, Quaternion.identity);
        AudioSource audioSource = soundObject.GetComponent<AudioSource>();
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(sound);
    }


    public void PlaySoundFootstep(Vector3 position, AudioClip sound, Transform toParent = null, AnimationCurve customCurve = null)
    {
        GameObject newSound = Instantiate(footstepPrefab, position, Quaternion.identity, toParent);
        AudioSource audioSource = newSound.GetComponent<AudioSource>();
        audioSource.PlayOneShot(sound);
    }

}
