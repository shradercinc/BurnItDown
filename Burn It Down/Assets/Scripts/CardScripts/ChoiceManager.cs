using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager instance;
    [HideInInspector] public Card chosenCard;
    [HideInInspector] public float opacity = 1;
    [HideInInspector] public bool decrease = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void FixedUpdate()
    {
        //dicates how cards flash when they can be chosen
        if (decrease)
            opacity -= 0.05f;
        else
            opacity += 0.05f;
        if (opacity < 0 || opacity > 1)
            decrease = !decrease;
    }

    public void ReceiveChoice(Card chosenCard)
    {
        Debug.Log($"chosen {chosenCard.name}");
        this.chosenCard = chosenCard;
    }

    public void ChooseCard(List<Card> choices)
    {
        chosenCard = null;
        //turn on all buttons that can be pressed
        for (int i = 0; i < choices.Count; i++)
            choices[i].choiceScript.EnableButton(true);
    }

    public void DisableCards()
    {
        //turn off all cards
        for (int i = 0; i < SaveManager.instance.allCards.Count; i++)
            SaveManager.instance.allCards[i].choiceScript.DisableButton();
    }
}
