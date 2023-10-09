using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckBuildManager : MonoBehaviour
{
    List<Transform> cardsInDeck = new List<Transform>();
    List<Transform> cardsInCollection = new List<Transform>();

    [SerializeField] RectTransform yourDeck;
    [SerializeField] RectTransform yourCollection;

    [SerializeField] int deckSize;
    [SerializeField] TMP_Text deckSizeText;
    [SerializeField] Button playGameButton;

    private void Start()
    {
        StartCoroutine(Setup());
    }

    private void Update()
    {
        playGameButton.gameObject.SetActive(yourDeck.childCount == deckSize);
    }

    public void AddToDeck(Transform newCard, bool save)
    {
        if (cardsInDeck.Count < deckSize)
        {
            //put that card on the top row
            cardsInCollection.Remove(newCard);
            cardsInDeck.Add(newCard);
            newCard.transform.SetParent(yourDeck);

            if (save)
                SaveManager.instance.SaveHand(cardsInDeck);
        }
    }

    public void RemoveFromDeck(Transform newCard, bool save)
    {
        //put that card on the bottom row
        cardsInDeck.Remove(newCard);
        cardsInCollection.Add(newCard);
        newCard.transform.SetParent(yourCollection);

        if (save)
            SaveManager.instance.SaveHand(cardsInDeck);
    }

    IEnumerator Setup()
    {
        yield return new WaitForSeconds(0.25f);

        //take all cards and put them on the top
        for (int i = 0; i < SaveManager.instance.allCards.Count; i++)
            RemoveFromDeck(SaveManager.instance.allCards[i].transform, false);

        if (SaveManager.instance.currentSaveData.chosenDeck != null)
            for (int i = 0; i < SaveManager.instance.currentSaveData.chosenDeck.Count; i++)
                AddToDeck(yourCollection.transform.Find(SaveManager.instance.currentSaveData.chosenDeck[i]), false);

        StartCoroutine(SwapCards());
    }

    IEnumerator SwapCards()
    {
        //choose a card
        deckSizeText.text = $"Your Deck ({yourDeck.childCount}/{deckSize})";
        ChoiceManager.instance.ChooseCard(SaveManager.instance.allCards);
        while (ChoiceManager.instance.chosenCard == null)
            yield return null;

        //swap cards between your deck and collection
        if (ChoiceManager.instance.chosenCard.transform.parent == yourCollection)
            AddToDeck(ChoiceManager.instance.chosenCard.transform, true);
        else
            RemoveFromDeck(ChoiceManager.instance.chosenCard.transform, true);

        yield return SwapCards();
    }
}
