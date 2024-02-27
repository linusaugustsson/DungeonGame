using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityButton : MonoBehaviour
{
    [Header("Image & Sprite")]
    public Image abilityImage;
    public Image abilityFrame;
    internal AbilityHolder abilityHolder;

    [Header("Text")]
    public TMP_Text abilityCooldownText;

    [Header("Colors")]
    public Color activeAbilityColor;
    public Color onCooldownAbilityColor;
    private Color defaultAbilityColor;

    [Header("Frames")]
    public Sprite defaultFrame;
    public Sprite selectedFrame;

    void Start()
    {
        defaultAbilityColor = abilityImage.color;
    }

    void Update()
    {
        if (abilityHolder == null)
        {
            return;
        }

        abilityFrame.sprite = abilityHolder.isSelected ? selectedFrame : defaultFrame;

        switch (abilityHolder.state)
        {
            case AbilityHolder.abillityState.ready:
                abilityCooldownText.text = "";
                abilityImage.color = defaultAbilityColor;
                break;
            case AbilityHolder.abillityState.active:
                abilityCooldownText.text = "";
                abilityImage.color = activeAbilityColor;
                break;
            case AbilityHolder.abillityState.cooldown:
                abilityImage.color = onCooldownAbilityColor;
                abilityImage.color = onCooldownAbilityColor;
                abilityCooldownText.text = abilityHolder.cooldownTime.ToString("F1");
                break;
            default:
                break;
        }

    }
}
