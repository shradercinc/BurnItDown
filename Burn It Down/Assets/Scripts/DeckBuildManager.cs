using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckBuildManager : MonoBehaviour
{
    List<Card> cardsInDeck = new List<Card>();
    List<Card> cardsInCollection = new List<Card>();

    [SerializeField] RectTransform yourDeck;
    [SerializeField] RectTransform yourCollection;

    [SerializeField] int deckSize;
    [SerializeField] TMP_Text deckSizeText;
    [SerializeField] Button playGameButton;

    private void Start()
    {
        Application.targetFrameRate = 60;
        RightClick.instance.transform.SetParent(this.transform.parent);
        StartCoroutine(Setup());
    }

    private void Update()
    {
        playGameButton.gameObject.SetActive(yourDeck.childCount == deckSize);
    }

    public void AddToDeck(Card newCard, bool save)
    {
        if (cardsInDeck.Count < 15)
        {
            //put that card on the top row
            cardsInCollection.Remove(newCard);
            cardsInDeck.Add(newCard);
            newCard.transform.SetParent(yourDeck);

            if (save)
                SaveManager.instance.SaveHand(cardsInDeck);
        }
    }

    public void RemoveFromDeck(Card newCard, bool save)
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

        //take all cards and put them on the bottom
        for (int i = 0; i < SaveManager.instance.allCards.Count; i++)
            RemoveFromDeck(SaveManager.instance.allCards[i], false);

        //take all cards already saved in your deck and put them on the top
        for (int i = 0; i < SaveManager.instance.newSaveData.chosenDeck.Count; i++)
            AddToDeck(SaveManager.instance.newSaveData.chosenDeck[i], false);

        StartCoroutine(SwapCards());
    }

    IEnumerator SwapCards()
    {
        deckSizeText.text = $"Your Deck ({yourDeck.childCount}/{deckSize})";
        ChoiceManager.instance.ChooseCard(SaveManager.instance.allCards);
        while (ChoiceManager.instance.chosenCard == null)
            yield return null;

        //swap cards between your deck and collection
        if (ChoiceManager.instance.chosenCard.transform.parent == yourCollection)
            AddToDeck(ChoiceManager.instance.chosenCard, true);
        else
            RemoveFromDeck(ChoiceManager.instance.chosenCard, true);

        yield return SwapCards();
    }
}
