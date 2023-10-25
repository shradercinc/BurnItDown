using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;

public class PlayerEntity : MovingEntity
{
    [Foldout("Player Entity", true)]
        [Tooltip("Where this player's located in the list")][ReadOnly] public int myPosition;
        [Tooltip("turns where you can't be caught")][ReadOnly] public int health = 3;
        [Tooltip("turns where you can't be caught")] [ReadOnly] public int hidden = 0;
        //[Tooltip("normal player appearance")] [SerializeField] Material DefaultPlayerMaterial;
        //[Tooltip("appearance when hidden")] [SerializeField] Material HiddenPlayerMaterial;
        [Tooltip("adjacent objective")][ReadOnly] public ObjectiveEntity adjacentObjective;

    [Foldout("Player's Cards", true)]
        [Tooltip("adjacent objective")][ReadOnly] public int myEnergy;
        [Tooltip("adjacent objective")][ReadOnly] public Transform handTransform;
        [Tooltip("adjacent objective")][ReadOnly] public List<Card> myHand;
        [Tooltip("adjacent objective")][ReadOnly] public List<Card> myDeck;
        [Tooltip("adjacent objective")][ReadOnly] public List<Card> myDiscardPile;

#region Entity stuff

    public override string HoverBoxText()
    {
        return $"Moves left: {movementLeft}";
    }

    public override void MoveTile(TileData newTile)
    {
        base.MoveTile(newTile);
        foreach (GuardEntity guard in NewManager.instance.listOfGuards)
        {
            guard.CheckForPlayer();
        }
        NewManager.instance.objectiveButton.gameObject.SetActive(CheckForObjectives());
    }

    public bool CheckForObjectives()
    {
        for (int i = 0; i < this.currentTile.adjacentTiles.Count; i++)
        {
            TileData nextTile = this.currentTile.adjacentTiles[i];
            if (nextTile.myEntity != null && nextTile.myEntity.CompareTag("Objective"))
            {
                this.adjacentObjective = nextTile.myEntity.GetComponent<ObjectiveEntity>();
                return adjacentObjective.CanInteract();
            }
        }

        return false;
    }

    public override IEnumerator EndOfTurn()
    {
        yield return null;
        if (hidden > 0)
            hidden--;

        //meshRenderer.material = (hidden > 0) ? HiddenPlayerMaterial : DefaultPlayerMaterial;
    }
    #endregion

    #region Card Stuff

    public void DrawCards(int num)
    {
        for (int i = 0; i < num; i++)
        {
            try
            {
                AddCardToHand(GetTopCard());
            }
            catch (NullReferenceException)
            {
                break;
            }
        }
    }

    public Card GetTopCard()
    {
        if (myDeck.Count == 0)
        {
            myDiscardPile.Shuffle();
            while (myDiscardPile.Count > 0)
            {
                myDeck.Add(myDiscardPile[0]);
                myDiscardPile.RemoveAt(0);
            }
        }

        if (myDeck.Count > 0) //get the top card of the deck if there is one
            return myDeck[0];
        else
            return null;
    }

    public void AddCardToHand(Card drawMe)
    {
        if (drawMe != null)
        {
            myHand.Add(drawMe);
            myDeck.Remove(drawMe);
            myDiscardPile.Remove(drawMe);

            drawMe.transform.SetParent(handTransform);
            drawMe.transform.localScale = new Vector3(1, 1, 1);
            SoundManager.instance.PlaySound(drawMe.cardMove);
        }
    }

    public void DiscardCard(Card discardMe)
    {
        if (discardMe != null)
        {
            myHand.Remove(discardMe);
            myDeck.Remove(discardMe);
            myDiscardPile.Add(discardMe);

            discardMe.transform.SetParent(null);
            discardMe.transform.localPosition = new Vector3(1000, 1000, 0); //send the card far away where you can't see it anymore
            SoundManager.instance.PlaySound(discardMe.cardMove);
        }
    }

    #endregion
}
