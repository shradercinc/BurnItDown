using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager instance;
    [HideInInspector] public float opacity = 1;
    [HideInInspector] public bool decrease = true;

    RectTransform handTransform;
    public List<Card> listOfHand = new List<Card>();

    Transform deck;

    Card chosenCard;

    private void Awake()
    {
        deck = GameObject.Find("Deck").transform;
        handTransform = this.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>();
        instance = this;
    }

    private void Start()
    {
        Debug.Log(handTransform.name);
        deck.Shuffle();
        DrawCards(4);
        StartCoroutine(ChooseCardInHand());
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

    public void DrawCards(int num)
    {
        for (int i = 0; i<num; i++)
            AddCardToHand(deck.GetChild(0).GetComponent<Card>());
    }

    public void AddCardToHand(Card newCard)
    {
        listOfHand.Add(newCard);
        newCard.transform.SetParent(handTransform);
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

    public IEnumerator ChooseCardInHand()
    {
        for (int i = 0; i < listOfHand.Count; i++)
            listOfHand[i].choiceScript.EnableButton(true);

        yield return WaitForDecision();

        for (int i = 0; i < listOfHand.Count; i++)
            listOfHand[i].choiceScript.DisableButton();
    }
}
