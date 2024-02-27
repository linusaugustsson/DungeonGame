using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionInfo : MonoBehaviour
{
    public static ConnectionInfo instance;

    public CanvasGroup canvasGroup;
    public TextMeshProUGUI infoText;

    public float fadeTime = 1.0f;
    public float holdTime = 3.0f;
    private float _timer = 0.0f;
    private float _holdTimer = 0.0f;

    private bool _unfade = false;

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

    private void Update()
    {
        UnFadeLogic();
    }


    public void ShowConnectionMessage(string newInfoText, float showInfoTime)
    {
        infoText.text = newInfoText;
        holdTime = showInfoTime;
        canvasGroup.alpha = 1.0f;
        _unfade = true;
    }

    private void UnFadeLogic()
    {
        if (_unfade)
        {

            if (_holdTimer < holdTime)
            {
                _holdTimer += Time.deltaTime;
                return;
            }

            if (_timer < fadeTime)
            {
                _timer += Time.deltaTime;
                float fadePercentage = Mathf.Lerp(1.0f, 0.0f, _timer / fadeTime);

                canvasGroup.alpha = fadePercentage;

                if (_timer >= fadeTime)
                {
                    _unfade = false;
                    _timer = 0.0f;
                    canvasGroup.alpha = 0.0f;
                    _holdTimer = 0.0f;
                }
            }
        }
    }


}
