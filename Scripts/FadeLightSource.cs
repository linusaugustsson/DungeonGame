using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeLightSource : MonoBehaviour
{
    public float speed = 1;
    public bool DestroyGameObjectOnFadeOut = true;
    public bool alsoFadeRange = true;
    [Space]

    public Light lightSource;

    void Update()
    {
        if (lightSource.intensity > 0)
        {
            lightSource.intensity -= Time.deltaTime * speed;

            if (alsoFadeRange)
            {
                lightSource.range -= Time.deltaTime * speed;
            }
        }
        else if (DestroyGameObjectOnFadeOut)
        {
            Destroy(gameObject);
        }
    }
}
