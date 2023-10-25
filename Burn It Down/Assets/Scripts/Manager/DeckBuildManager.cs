using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckBuildManager : MonoBehaviour
{
    List<List<Card>> cardsInDeck = new List<List<Card>>();
    List<List<Card>> cardsInCollection = new List<List<Card>>();

    Transform handContainer;
    List<Transform> deckTransforms = new List<Transform>();
    List<Transform> collectionTransforms = new List<Transform>();

    TMP_Dropdown dropdown;
    TMP_Text deckSizeText;
    Button playGameButton;

    [SerializeField] int deckSize;
    [SerializeField] AudioClip cardMove;

    int currentDeck = 0;

    private void Awake()
    {
        dropdown = GameObject.Find("Dropdown").GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(delegate { NewSort(); });
        deckSizeText = GameObject.Find("Deck Text").GetComponent<TMP_Text>();
        playGameButton = GameObject.Find("Play Game Button").GetComponent<Button>();

        handContainer = GameObject.Find("All Cards").transform;
        foreach(Transform child in handContainer.GetChild(0))
            collectionTransforms.Add(child);
        foreach (Transform child in handContainer.GetChild(1))
            deckTransforms.Add(child);
    }

    private void Start()
    {
        for (int i = 0; i<SaveManager.instance.characterCards.Count; i++)
        {
            currentDeck = i;
            foreach(Card card in SaveManager.instance.characterCards[i])
            {
                card.transform.localScale = new Vector3(1, 1, 1);
                RemoveFromDeck(card, false);
            }
        }

        for (int i = 0; i < SaveManager.instance.currentSaveData.savedDecks.Count; i++)
        {
            currentDeck = i;
            foreach(string card in SaveManager.instance.currentSaveData.savedDecks[i])
            {
                AddToDeck(collectionTransforms[currentDeck].Find(card).GetComponent<Card>(), false);
            }
        }

        currentDeck = 0;
        StartCoroutine(SwapCards());
    }

    public void NewSort()
    {
        foreach (List<Card> cardList in cardsInCollection)
        {
            foreach (Card card in cardList)
            {
                ApplySorting(card);
            }
        }
    }

    void ApplySorting(Card card)
    {
        switch (dropdown.options[dropdown.value].text)
        {
            case "":
                card.gameObject.SetActive(true);
                break;
            case "All cards":
                card.gameObject.SetActive(true);
                break;
            case "Costs 0":
                card.gameObject.SetActive(card.energyCost == 0);
                break;
            case "Costs 1":
                card.gameObject.SetActive(card.energyCost == 1);
                break;
            case "Costs 2":
                card.gameObject.SetActive(card.energyCost == 2);
                break;
            case "Costs 3":
                card.gameObject.SetActive(card.energyCost == 3);
                break;
            case "Attack":
                card.gameObject.SetActive(card.typeOne == Card.CardType.Attack || card.typeTwo == Card.CardType.Attack);
                break;
            case "Draw":
                card.gameObject.SetActive(card.typeOne == Card.CardType.Draw || card.typeTwo == Card.CardType.Draw);
                break;
            case "Energy":
                card.gameObject.SetActive(card.typeOne == Card.CardType.Energy || card.typeTwo == Card.CardType.Energy);
                break;
            case "Movement":
                card.gameObject.SetActive(card.typeOne == Card.CardType.Movement || card.typeTwo == Card.CardType.Movement);
                break;
            case "Misc effect":
                card.gameObject.SetActive(card.typeOne == Card.CardType.Misc || card.typeTwo == Card.CardType.Misc);
                break;
            default:
                Debug.LogError("filter not implemented");
                break;
        }
    }

    public void AddToDeck(Card newCard, bool save)
    {
        if (cardsInDeck[currentDeck].Count < deckSize)
        {
            //put that card on the top row
            cardsInCollection[currentDeck].Remove(newCard);
            cardsInDeck[currentDeck].Add(newCard);
            newCard.transform.SetParent(deckTransforms[currentDeck]);
            ApplySorting(newCard);

            SoundManager.instance.PlaySound(cardMove);

            if (save)
                SaveManager.instance.SaveHand(cardsInDeck);
        }
    }

    public void RemoveFromDeck(Card newCard, bool save)
    {
        playGameButton.gameObject.SetActive(false);
        cardsInDeck[currentDeck].Remove(newCard);
        cardsInCollection[currentDeck].Add(newCard);
        newCard.transform.SetParent(collectionTransforms[currentDeck]);

        SoundManager.instance.PlaySound(cardMove);
        if (save)
            SaveManager.instance.SaveHand(cardsInDeck);
    }

    IEnumerator SwapCards()
    {
        deckSizeText.text = $"Your Deck ({cardsInDeck[currentDeck].Count}/{deckSize})";
        foreach(List<Card> cardList in SaveManager.instance.characterCards)
            ChoiceManager.instance.ChooseCard(cardList);

        while (ChoiceManager.instance.chosenCard == null)
            yield return null;

        //swap cards between your deck and collection
        if (cardsInCollection[currentDeck].Contains(ChoiceManager.instance.chosenCard))
            AddToDeck(ChoiceManager.instance.chosenCard, true);
        else
            RemoveFromDeck(ChoiceManager.instance.chosenCard, true);

        StartCoroutine(SwapCards());
    }
}
