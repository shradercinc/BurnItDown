using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuildManager : MonoBehaviour
{
    List<Card> cardsInDeck = new List<Card>();
    List<Card> cardsInCollector = new List<Card>();

    public RectTransform yourDeck;
    public RectTransform yourCollection;

    private void Start()
    {
        StartCoroutine(Setup());
    }

    public void AddToDeck(Card newCard, bool save)
    {
        cardsInCollector.Remove(newCard);
        cardsInDeck.Add(newCard);
        newCard.transform.SetParent(yourDeck);

        if (save)
            SaveManager.instance.SaveDeck(cardsInDeck);
    }

    public void RemoveFromDeck(Card newCard, bool save)
    {
        cardsInDeck.Remove(newCard);
        cardsInCollector.Add(newCard);
        newCard.transform.SetParent(yourCollection);

        if (save)
            SaveManager.instance.SaveDeck(cardsInDeck);
    }

    IEnumerator Setup()
    {
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < SaveManager.instance.allCards.Count; i++)
            RemoveFromDeck(SaveManager.instance.allCards[i], false);

        for (int i = 0; i < SaveManager.instance.newSaveData.savedDeck.Count; i++)
            AddToDeck(SaveManager.instance.newSaveData.savedDeck[i], false);

        StartCoroutine(SwapCards());
    }

    IEnumerator SwapCards()
    {
        yield return ChoiceManager.instance.ChooseCard(SaveManager.instance.allCards);

        if (ChoiceManager.instance.chosenCard.transform.parent == yourDeck)
            RemoveFromDeck(ChoiceManager.instance.chosenCard, true);
        else
            AddToDeck(ChoiceManager.instance.chosenCard, true);

        yield return SwapCards();
    }
}
