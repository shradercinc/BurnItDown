using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager instance;
    RectTransform handTransform;
    public List<Card> listOfHand = new List<Card>();

    Transform deck;
    Transform discardPile;

    private void Awake()
    {
        deck = GameObject.Find("Deck").transform;
        discardPile = GameObject.Find("Discard Pile").transform;
        handTransform = this.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>();
        instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < SaveManager.instance.newSaveData.savedDeck.Count; i++)
            SaveManager.instance.newSaveData.savedDeck[i].transform.SetParent(deck);

        deck.Shuffle();
        DrawCards(4);

        StartCoroutine(PlayGame());
    }

    public void DrawCards(int num)
    {
        for (int i = 0; i < num; i++)
        {
            if (deck.childCount > 0)
                AddCardToHand(deck.GetChild(0).GetComponent<Card>());
        }
    }

    public void AddCardToHand(Card newCard)
    {
        listOfHand.Add(newCard);
        newCard.transform.SetParent(handTransform);
    }

    IEnumerator PlayGame()
    {
        yield return ChoiceManager.instance.ChooseCard(listOfHand);
        DiscardCard(ChoiceManager.instance.chosenCard);
    }

    public void DiscardCard(Card discardMe)
    {
        discardMe.transform.SetParent(discardPile);
        listOfHand.Remove(discardMe);
        discardMe.transform.localPosition = new Vector3(1000, 1000, 0);
    }
}
