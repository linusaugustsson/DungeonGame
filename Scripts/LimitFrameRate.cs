using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LimitFrameRate : MonoBehaviour
{
    [Range(1, 300)]
    [SerializeField] private int frameRate = 60;

    void Start()
    {
        #if UNITY_EDITOR
        if (frameRate > 0)
            frameRate = 1;

        if (frameRate < 300)
            frameRate = 300;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = frameRate;
        #endif
    }

}
