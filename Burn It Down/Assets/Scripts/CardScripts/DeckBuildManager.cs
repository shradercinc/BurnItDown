using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuildManager : MonoBehaviour
{
    List<Card> cardsInHand = new List<Card>();
    List<Card> cardsInDeck = new List<Card>();

    public RectTransform yourHand;
    public RectTransform yourDeck;

    [SerializeField] AudioClip cardMoveSound;

    private void Start()
    {
        RightClick.instance.transform.SetParent(this.transform.parent);
        StartCoroutine(Setup());
    }

    public void AddToHand(Card newCard, bool save)
    {
        if (cardsInHand.Count < 5)
        {
            //put that card on the top row
            cardsInDeck.Remove(newCard);
            cardsInHand.Add(newCard);
            newCard.transform.SetParent(yourHand);
            SoundManager.instance.PlaySound(cardMoveSound);

            if (save)
                SaveManager.instance.SaveHand(cardsInHand);
        }
    }

    public void RemoveFromHand(Card newCard, bool save)
    {
        //put that card on the bottom row
        cardsInHand.Remove(newCard);
        cardsInDeck.Add(newCard);
        newCard.transform.SetParent(yourDeck);
        SoundManager.instance.PlaySound(cardMoveSound);

        if (save)
            SaveManager.instance.SaveHand(cardsInHand);
    }

    IEnumerator Setup()
    {
        yield return new WaitForSeconds(0.25f);

        //take all cards and put them on the bottom
        for (int i = 0; i < SaveManager.instance.allCards.Count; i++)
            RemoveFromHand(SaveManager.instance.allCards[i], false);

        //take all cards already saved in your deck and put them on the top
        for (int i = 0; i < SaveManager.instance.newSaveData.startingHand.Count; i++)
            AddToHand(SaveManager.instance.newSaveData.startingHand[i], false);

        StartCoroutine(SwapCards());
    }

    IEnumerator SwapCards()
    {
        ChoiceManager.instance.ChooseCard(SaveManager.instance.allCards);
        while (ChoiceManager.instance.chosenCard == null)
            yield return null;

        //swap cards between your deck and collection
        if (ChoiceManager.instance.chosenCard.transform.parent == yourDeck)
            AddToHand(ChoiceManager.instance.chosenCard, true);
        else
            RemoveFromHand(ChoiceManager.instance.chosenCard, true);

        yield return SwapCards();
    }
}
