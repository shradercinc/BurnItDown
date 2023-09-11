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
        if (decrease)
            opacity -= 0.05f;
        else
            opacity += 0.05f;
        if (opacity < 0 || opacity > 1)
            decrease = !decrease;
    }

    public void ReceiveChoice(Card chosenCard)
    {
        this.chosenCard = chosenCard;
    }

    public IEnumerator WaitForDecision()
    {
        chosenCard = null;
        while (chosenCard == null)
            yield return null;
    }

    public IEnumerator ChooseCard(List<Card> choices)
    {
        for (int i = 0; i < choices.Count; i++)
            choices[i].choiceScript.EnableButton(true);

        yield return WaitForDecision();

        for (int i = 0; i < choices.Count; i++)
            choices[i].choiceScript.DisableButton();
    }
}
