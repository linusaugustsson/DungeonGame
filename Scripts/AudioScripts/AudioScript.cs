using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    private bool hasStartedPlaying = false;
    private AudioSource myAudio;

    private void Awake()
    {
        myAudio = GetComponent<AudioSource>();
    }

    private void Update()
    {

        if (myAudio.isPlaying)
        {
            hasStartedPlaying = true;
        }

        if (myAudio.isPlaying == false && hasStartedPlaying == true)
        {
            Destroy(this.gameObject);
        }
    }


}
