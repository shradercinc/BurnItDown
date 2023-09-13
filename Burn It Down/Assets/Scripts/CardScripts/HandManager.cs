using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager instance;
    RectTransform handTransform; //there are 2 things that store your hand, just to make things easier
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
        //get the cards you put in your deck
        for (int i = 0; i < SaveManager.instance.newSaveData.savedDeck.Count; i++)
            SaveManager.instance.newSaveData.savedDeck[i].transform.SetParent(deck);

        deck.Shuffle(); //shuffle that deck
        DrawCards(4); //draw some number of cards

        StartCoroutine(PlayGame());
    }

    public void DrawCards(int num)
    {
        for (int i = 0; i < num; i++)
        {
            if (deck.childCount > 0) //get the top card of the deck if there is one
                AddCardToHand(deck.GetChild(0).GetComponent<Card>());
        }
    }

    public void AddCardToHand(Card newCard)
    {
        //add the new card to your hand
        listOfHand.Add(newCard);
        newCard.transform.SetParent(handTransform);
    }

    IEnumerator PlayGame()
    {
        //a quick demo for choosing a card and discarding it
        yield return ChoiceManager.instance.ChooseCard(listOfHand);
        DiscardCard(ChoiceManager.instance.chosenCard);
    }

    public void DiscardCard(Card discardMe)
    {
        discardMe.transform.SetParent(discardPile);
        listOfHand.Remove(discardMe);
        discardMe.transform.localPosition = new Vector3(1000, 1000, 0); //send the card far away where you can't see it anymore
    }

    public void UnlockedCard(Card unlocked)
    {
        SaveManager.instance.newSaveData.unlockedCards.Add(unlocked);
        unlocked.gameObject.SetActive(true);
    }
}
