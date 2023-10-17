using System.Collections;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DeckBuildManager : MonoBehaviour
{
    List<Card> cardsInDeck = new List<Card>();
    List<Card> cardsInCollection = new List<Card>();

    [SerializeField] TMP_Dropdown dropdown;

    [SerializeField] RectTransform yourDeck;
    [SerializeField] RectTransform yourCollection;

    [SerializeField] int deckSize;
    [SerializeField] TMP_Text deckSizeText;
    [SerializeField] Button playGameButton;
    [SerializeField] AudioClip cardMove;

    private void Awake()
    {
        dropdown = GameObject.Find("Dropdown").GetComponent<TMP_Dropdown>();
        dropdown.onValueChanged.AddListener(delegate { NewSort(); });
    }

    private void Start()
    {
        if (SaveManager.instance != null)
            StartCoroutine(Setup());
        else
            SceneManager.LoadScene(0);
    }

    public void NewSort()
    {
        foreach (Card card in cardsInCollection)
        {
            ApplySorting(card);
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

    private void Update()
    {
        playGameButton.gameObject.SetActive(yourDeck.childCount == deckSize);
    }

    public void AddToDeck(Card newCard, bool save)
    {
        if (cardsInDeck.Count < deckSize)
        {
            //put that card on the top row
            cardsInCollection.Remove(newCard);
            cardsInDeck.Add(newCard);
            newCard.transform.SetParent(yourDeck);
            ApplySorting(newCard);

            SoundManager.instance.PlaySound(cardMove);

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

        SoundManager.instance.PlaySound(cardMove);

        if (save)
            SaveManager.instance.SaveHand(cardsInDeck);
    }

    IEnumerator Setup()
    {
        yield return new WaitForSeconds(0.1f);

        foreach (Card card in SaveManager.instance.allCards)
        {
            card.transform.localScale = new Vector3(1, 1, 1);
            RemoveFromDeck(card, false);
        }

        if (SaveManager.instance.currentSaveData.chosenDeck != null)
        {
            for (int i = 0; i < SaveManager.instance.currentSaveData.chosenDeck.Count; i++)
                AddToDeck(yourCollection.transform.Find(SaveManager.instance.currentSaveData.chosenDeck[i]).GetComponent<Card>(), false);
        }
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
