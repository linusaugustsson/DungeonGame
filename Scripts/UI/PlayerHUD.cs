using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    [Header("Health Bar")]
    public Slider healthBar;
    public TMP_Text healthText;
    [Space]

    [Header("Action Bar")]
    public Transform actionBarTransform;
    public Slider actionBar;
    public TMP_Text ActionTimeText;
    public TMP_Text ActionNameText;
    public Image ActionIcon;
    [Space]

    [Header("Generic Action icon")]
    public Sprite actionIconGeneric;
    [Space]

    [Header("Ability Bar")]
    public Transform abilityBarTransform;
    public AbilityButton[] abilityButtons;

    [Header("Buff & Debuff Bar")]
    public Transform buffDebuffBar;
    public Transform buffDebuffElemenetPrefab;

    void Start()
    {
        actionBarTransform.gameObject.SetActive(false);
    }

    void Update()
    {
        
    }

    public void updateHealthBar(float maxhealth, float currentHealth)
    {
        healthBar.maxValue = (int)maxhealth;
        healthBar.minValue = 0;

        /*
        float value = maxhealth - currentHealth / 100;

        if (currentHealth >= maxhealth)
        {
            value = maxhealth / 100;
        }
        */
        

        healthBar.value = currentHealth;

        healthText.text = currentHealth.ToString("F0");
    }

    public void updateActionBar(string name, float timeToComplete, float currentTime, Sprite actionSprite = null)
    {
        if (actionBarTransform.gameObject.activeInHierarchy == false)
        {
            actionBarTransform.gameObject.SetActive(true);
        }

        actionBar.maxValue = timeToComplete;

        actionBar.value = currentTime;

        ActionTimeText.text = currentTime.ToString("F1") + "s";
        ActionNameText.text = name;

        ActionIcon.sprite = actionSprite == null ? actionIconGeneric : actionSprite;

        if (currentTime >= timeToComplete)
        {
            actionBar.value = 0;
            actionBarTransform.gameObject.SetActive(false);
        }
    }

    public void CloseActionBar()
    {
        actionBar.value = 0;
        actionBarTransform.gameObject.SetActive(false);
    }

    public BuffDeBuffElement CreateBuffDeBuffElement()
    {
        var element = Instantiate(buffDebuffElemenetPrefab, buffDebuffBar);
        return element.GetComponent<BuffDeBuffElement>();
    }
}
