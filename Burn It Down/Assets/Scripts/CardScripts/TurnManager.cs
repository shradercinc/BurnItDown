using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;
    RectTransform handTransform; //there are 2 ways that track your hand, just to make things easier
    public List<Card> listOfHand = new List<Card>();

    int currentEnergy;
    int currentHealth;

    Transform deck;
    Transform discardPile;
    Transform exhausted;

    Slider healthBar;
    TMP_Text healthText;
    Slider energyBar;
    TMP_Text energyText;

    public TMP_Text gameOverText;

    private void Awake()
    {
        deck = GameObject.Find("Deck").transform;
        discardPile = GameObject.Find("Discard Pile").transform;
        exhausted = GameObject.Find("Exhausted").transform;

        healthBar = GameObject.Find("Health Slider").GetComponent<Slider>();
        healthText = healthBar.transform.GetChild(2).GetComponent<TMP_Text>();
        energyBar = GameObject.Find("Energy Slider").GetComponent<Slider>();
        energyText = energyBar.transform.GetChild(2).GetComponent<TMP_Text>();

        handTransform = this.transform.GetChild(0).transform.GetChild(0).GetComponent<RectTransform>();
        instance = this;
    }

    private void Start()
    {
        gameOverText.transform.parent.gameObject.SetActive(false);

        //since the right click script is under dontdestroyonload, we have to bring it back to the canvas
        RightClick.instance.transform.SetParent(this.transform.parent);

        //get the cards you added to your deck
        for (int i = 0; i < SaveManager.instance.newSaveData.savedDeck.Count; i++)
        {
            Card nextCard = SaveManager.instance.newSaveData.savedDeck[i];
            nextCard.choiceScript.DisableButton();
            nextCard.transform.SetParent(deck);
            nextCard.transform.localPosition = new Vector3(10000, 10000, 0); //send the card far away where you can't see it anymore
        }

        deck.Shuffle(); //shuffle that deck
        ChangeHealth(3);
        StartCoroutine(TakeTurn());
    }

    IEnumerator TakeTurn()
    {
        yield return null;
        DrawCards(5 - listOfHand.Count);
        ChangeEnergy((int)energyBar.maxValue - currentEnergy);

        //yield return ChoiceManager.instance.ChooseCard(listOfHand);
        //DiscardCard(ChoiceManager.instance.chosenCard);
    }

    public void ChangeHealth(int n)
    {
        currentHealth += n;
        healthText.text = $"Health: {currentHealth}";
        healthBar.value = currentHealth;
    }

    public void ChangeEnergy(int n)
    {
        currentEnergy += n;
        energyText.text = $"Energy: {currentEnergy}";
        energyBar.value = currentEnergy;
    }

    public void DrawCards(int num)
    {
        for (int i = 0; i < num; i++)
        {
            //if deck's empty, shuffle discard pile and add those cards to the deck
            //exhausted cards are not included in the shuffle

            if (deck.childCount > 0)
            {
                discardPile.Shuffle();
                while (discardPile.childCount > 0)
                    discardPile.GetChild(0).SetParent(deck);
            }

            if (deck.childCount > 0) //get the top card of the deck if there is one
                AddCardToHand(deck.GetChild(0).GetComponent<Card>());
        }
    }

    public void AddCardToHand(Card newCard)
    {
        //add the new card to your hand
        listOfHand.Add(newCard);
        newCard.transform.SetParent(handTransform);
        newCard.transform.localScale = new Vector3(1, 1, 1);
    }

    public void DiscardCard(Card discardMe)
    {
        discardMe.transform.SetParent(discardPile);
        listOfHand.Remove(discardMe);
        discardMe.transform.localPosition = new Vector3(1000, 1000, 0); //send the card far away where you can't see it anymore
    }

    public void ExhaustCard(Card exhaustMe)
    {
        exhaustMe.transform.SetParent(exhausted);
        listOfHand.Remove(exhaustMe);
        exhaustMe.transform.localPosition = new Vector3(10000, 10000, 0); //send the card far away where you can't see it anymore
    }

    public void UnlockedCard(Card unlocked)
    {
        SaveManager.instance.newSaveData.unlockedCards.Add(unlocked);
        unlocked.gameObject.SetActive(true);
    }

    public void GameOver(string cause)
    {
        gameOverText.gameObject.SetActive(true);
        gameOverText.text = cause;
    }
}
