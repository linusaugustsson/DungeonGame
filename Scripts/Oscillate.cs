using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillate : MonoBehaviour
{
    public float amp = 1f;
    public float freq = 1f;
    Vector3 initPos;
    [Space]
    public Vector2 randomStartDelayTime;

    float t = 0;
    float timer;

    private void Start()
    {
        if (randomStartDelayTime.x >= 0 && randomStartDelayTime.y > 0)
        {
            timer = Random.Range(randomStartDelayTime.x, randomStartDelayTime.y);
        }

        initPos = transform.position;
    }

    private void Update()
    {
        if (t < timer)
        {
            t += Time.deltaTime;
        }
        else
        {
            transform.position = new Vector3(transform.position.x, Mathf.Sin(Time.time * freq) * amp + initPos.y, transform.position.z);
        }

    }
}
