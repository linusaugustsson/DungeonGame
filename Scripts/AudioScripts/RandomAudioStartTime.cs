using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAudioStartTime : MonoBehaviour
{

    private AudioSource _audioSource;
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }


    private void Start()
    {
        _audioSource.time = Random.Range(0f, _audioSource.clip.length);
        _audioSource.Play();
    }
}
