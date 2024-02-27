using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class CharacterSheetMenu : MonoBehaviour
{
    #region Unity Object References
    public Image characterView;
    public TextMeshProUGUI characterText;
    public TextMeshProUGUI hoveredAbilityText;

    public AbilitySelectionItem abilityMenuIconPrefab;
    public Transform avalibleAbilitiesTransform;

    public Transform selectedAbilitiesTransform;
    #endregion

    int currentClassIndex;
    private List<Class> avalibleClasses => Class.classDatabase;

    private List<AbilitySelectionItem> avalibleAbilityItems = new();
    private List<(Ability ability, AbilitySelectionItem abilityItem)> selectedAbilitySets = new();

    private SessionManager sessionManager;

    public Class CurrentClass => avalibleClasses[currentClassIndex];
    private void Awake()
    {
        sessionManager = SteamLobby.instance.sessionManager;
        UpdateCharacterView();

        AbilitySelectionItem.OnAbilitySelectionChange += OnAbilitySelectionChanged;
        AbilitySelectionItem.OnAbilityIconHoveredChange += OnAbilityIconHoveredChange;
    }

    private void OnDestroy()
    {
        AbilitySelectionItem.OnAbilitySelectionChange -= OnAbilitySelectionChanged;
    }

    private void OnAbilityIconHoveredChange(AbilitySelectionItem abilityItem, bool hovering)
    {
        hoveredAbilityText.text = hovering ? abilityItem.ability.abilityName : "";
    }


    private void OnAbilitySelectionChanged(AbilitySelectionItem abilityItem)
    {
        var ability = abilityItem.ability;
        if (!abilityItem.isSelected)
        {
            if (selectedAbilitySets.Count < CurrentClass.nrOfAbilitySlots && !selectedAbilitySets.Exists((a) => a.ability == ability))
            {
                abilityItem.isSelected = true;
                var abilityIcon = Instantiate(abilityMenuIconPrefab, selectedAbilitiesTransform);
                abilityIcon.ability = ability;
                abilityIcon.isSelected = true;
                selectedAbilitySets.Add((ability, abilityIcon));
            }
        }
        else
        {
            abilityItem.isSelected = false;
            int abilitySetIndex = selectedAbilitySets.FindIndex((a) => a.ability == ability);
            if(abilitySetIndex != -1)
            {
                avalibleAbilityItems.Find((a) => a.ability == ability).isSelected = false;
                Destroy(selectedAbilitySets[abilitySetIndex].abilityItem.gameObject);
                selectedAbilitySets.RemoveAt(abilitySetIndex);
            }
        }
    }

    public void SetCharacter(int charIndex)
    {
        currentClassIndex = charIndex;
        UpdateCharacterView();
    }

    public void NextCharacter()
    {
        currentClassIndex = avalibleClasses.GetNextIndex(currentClassIndex);
        UpdateCharacterView();
    }

    public void PreviousCharacter()
    {
        currentClassIndex = avalibleClasses.GetPrevioustIndex(currentClassIndex);
        UpdateCharacterView();
    }

    private void UpdateCharacterView()
    {
        Class curClass = CurrentClass;
        characterView.sprite = curClass.classViewSprite;
        characterText.text = curClass.className;

        avalibleAbilityItems.ForEach((a) => Destroy(a.gameObject));
        avalibleAbilityItems.Clear();

        selectedAbilitySets.ForEach((a) => Destroy(a.abilityItem.gameObject));
        selectedAbilitySets.Clear();

        foreach (var ability in CurrentClass.avalibleClassAbilities)
        {
            var abilityIcon = Instantiate(abilityMenuIconPrefab, avalibleAbilitiesTransform);
            abilityIcon.ability = ability;
            avalibleAbilityItems.Add(abilityIcon);
        }
    }

    public struct CharacterData
    {
        public Class characterClass;
        public List<Ability> selectedAbilities;
    }

    public void ConfirmCharacter()
    {
        sessionManager.CollectPlayers();
        for (int i = 0; i < sessionManager.allPlayers.Count; i++)
        {
            if (sessionManager.allPlayers[i].IsOwner)
            {
                sessionManager.allPlayers[i].UpdateClass(currentClassIndex, selectedAbilitySets.ConvertAll<int>(a => Ability.abilityDatabase.FindIndex((b) => b == a.ability)));
            }
        }
        MenuManager.instance.inLobby.SetActive(true);
        gameObject.SetActive(false);
    }

}

public static class IListExtension
{
    public static int GetNextIndex(this IList list, int currentIndex, bool loopOnEndOfList = true)
    {
        if (currentIndex + 1 >= list.Count)
        {
            return 0;
        }
        else
        {
            return currentIndex + 1;
        }
    }

    public static int GetPrevioustIndex(this IList list, int currentIndex, bool loopOnStartOfList = true)
    {
        if (currentIndex <= 0)
        {
            return list.Count;
        }
        else
        {
            return currentIndex - 1;
        }
    }
}
