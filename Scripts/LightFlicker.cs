using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{

    private Light _light;

    public float minIntensity = 0.89f;
    public float maxIntensity = 1.0f;

    public float smoothTime = 0.1f;

    private float _velocity = 0.0f;


    private void Awake()
    {
        _light = GetComponent<Light>();
    }



    private void FixedUpdate()
    {
        float targetIntensity = Random.Range(minIntensity, maxIntensity);
        //float randomIntensity = Mathf.Lerp()
        float newIntensity = Mathf.SmoothDamp(_light.intensity, targetIntensity, ref _velocity, smoothTime);
        _light.intensity = newIntensity;
    }

}
