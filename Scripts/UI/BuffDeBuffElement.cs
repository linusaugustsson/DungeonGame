using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuffDeBuffElement : MonoBehaviour
{ 
    public Image buffDebuffIcon;
    public TMP_Text buffDebuffDurationText;

    public void UpdateBuffDebuffDuration(float time)
    {
        buffDebuffDurationText.text = time.ToString("F1") + "s";
    }
}
