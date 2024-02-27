using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FramerateCounter : MonoBehaviour
{
    private int lastFrameIndex;
    private float[] frameDeltaTimeArray;
    private TextMeshProUGUI uiText;

    private void Awake()
    {
        frameDeltaTimeArray = new float[50];
        uiText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        frameDeltaTimeArray[lastFrameIndex] = Time.unscaledDeltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % frameDeltaTimeArray.Length;

        uiText.text = Mathf.RoundToInt(CalculateFPS()).ToString();
    }


    private float CalculateFPS()
    {
        float total = 0.0f;
        foreach (float deltaTime in frameDeltaTimeArray)
        {
            total += deltaTime;
        }

        return frameDeltaTimeArray.Length / total;
    }

}
