using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using UnityEngine.EventSystems;

public class StringAndMethod
{
    [ReadOnly] public Dictionary<string, IEnumerator> dictionary = new Dictionary<string, IEnumerator>();

    public StringAndMethod(Card card)
    {
        dictionary["DRAWCARDS"] = card.DrawCards();
        dictionary["CHANGEHP"] = card.ChangeHealth();
        dictionary["CHANGEEP"] = card.ChangeEnergy();
        dictionary["CHANGEMP"] = card.ChangeMovement();
        dictionary["FINDONE"] = card.FindOne();
        dictionary["DISCARDHAND"] = card.DiscardHand();
        dictionary["STUNADJACENTGUARD"] = card.StunAdjacentGuard();
        dictionary["AFFECTADJACENTWALL"] = card.AffectAdjacentWall();
    }
}

public class Card : MonoBehaviour, IPointerClickHandler
{
#region Card Stats

    [ReadOnly] public Image image;
    [ReadOnly] public SendChoice choiceScript;

    [ReadOnly] public int energyCost;
    public enum CardType { Attack, Draw, Distraction, Energy, Movement, Misc, None };
    [ReadOnly] public CardType typeOne;
    [ReadOnly] public CardType typeTwo;
    [ReadOnly] public bool violent;

    [ReadOnly] int changeInHP;
    [ReadOnly] int changeInMP;
    [ReadOnly] int changeInEP;
    [ReadOnly] int changeInDraw;

    [ReadOnly] int stunDuration;
    [ReadOnly] int range;
    [ReadOnly] int areaOfEffect;
    [ReadOnly] int delay;
    [ReadOnly] int changeInWall;
    [ReadOnly] int burnDuration;
    [ReadOnly] int distractionIntensity;

    public enum CanPlayCondition { None, Guard, Wall, Occupied };
    [ReadOnly] CanPlayCondition selectCondition;
    [ReadOnly] string effectsInOrder;
    [ReadOnly] string nextRoundEffectsInOrder;

    [ReadOnly] public TMP_Text TextName { get; private set; } 
    [ReadOnly] public TMP_Text TextCost { get; private set; }
    [ReadOnly] public TMP_Text TextDescr { get; private set; }

    [ReadOnly] PlayerEntity currentPlayer;
    [ReadOnly] List<TileData> adjacentTilesWithGuards = new List<TileData>();
    [ReadOnly] List<TileData> adjacentTilesWithWalls = new List<TileData>();

    public AudioClip cardMove;
    public AudioClip cardPlay;

    int debugger = 0;
    
#endregion

#region Setup

    private void Awake()
    {
        image = GetComponent<Image>();
        choiceScript = GetComponent<SendChoice>();

        TextName = this.transform.GetChild(1).GetComponent<TMP_Text>();
        TextCost = this.transform.GetChild(2).GetComponent<TMP_Text>();
        TextDescr = this.transform.GetChild(3).GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            RightClick.instance.ChangeCard(this);
            SoundManager.instance.PlaySound(cardMove);
        }
    }

    public void CardSetup(CardData data)
    {
        name = data.name;
        TextName.text = data.name;
        TextDescr.text = data.desc;

        typeOne = ConvertToType(data.cat1);
        typeTwo = ConvertToType(data.cat2);

        energyCost = data.epCost;
        TextCost.text = $"{data.epCost}";
        violent = data.isViolent;

        changeInHP = data.chHP;
        changeInMP = data.chMP;
        changeInEP = data.chEP;
        changeInDraw = data.draw;

        stunDuration = data.stun;
        range = data.range;
        areaOfEffect = data.aoe;
        delay = data.delay;
        changeInWall = data.wHP;

        burnDuration = data.burn;
        distractionIntensity = data.intn;

        selectCondition = ConvertToCondition(data.select);
        effectsInOrder = data.action;
        nextRoundEffectsInOrder = data.nextAct;
    }

    CardType ConvertToType(string type)
    {
        return type switch
        {
            "draw" => CardType.Draw,
            "atk" => CardType.Attack,
            "dist" => CardType.Distraction,
            "eng" => CardType.Energy,
            "mvmt" => CardType.Movement,
            "misc" => CardType.Misc,
            _ => CardType.None,
        };
    }

    CanPlayCondition ConvertToCondition(string condition)
    {
        return condition switch
        {
            "isGuard" => CanPlayCondition.Guard,
            "isWall" => CanPlayCondition.Wall,
            "isOccupied" => CanPlayCondition.Occupied,
            _ => CanPlayCondition.None,
        };
    }

#endregion

#region Play Condition

    public bool CanPlay(PlayerEntity player)
    {
        currentPlayer = player;
        image.color = Color.white;
        if (Check(player))
        {
            image.color = Color.white;
            return true;
        }
        else
        {
            image.color = Color.gray;
            return false;
        }
    }

    bool Check(PlayerEntity player)
    {
        if (NewManager.instance.EnoughEnergy(energyCost))
        {
            return selectCondition switch
            {
                CanPlayCondition.Guard => SearchAdjacentGuard(player.currentTile).Count > 0,
                CanPlayCondition.Wall => SearchAdjacentWall(player.currentTile).Count > 0,
                CanPlayCondition.Occupied => (OccupiedAdjacent(player.currentTile).Count > 0),
                _ => true,
            };
        }
         
        {
            return false;
        }
    }

    List<TileData> OccupiedAdjacent(TileData playerTile)
    {
        List<TileData> occupiedTiles = new List<TileData>();
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(playerTile, range, false);

        foreach (TileData tile in tilesInRange)
        {
            if (tile.myEntity != null)
                occupiedTiles.Add(tile);
        }
        return occupiedTiles;
    }

    List<TileData> SearchAdjacentGuard(TileData playerTile)
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(playerTile, range, false);
        List<TileData> guardsInRange = new List<TileData>();

        foreach(TileData tile in tilesInRange)
        {
            if (tile.myEntity != null && tile.myEntity.CompareTag("Enemy"))
                guardsInRange.Add(tile);
        }

        adjacentTilesWithGuards = guardsInRange;
        return guardsInRange;
    }

    List<TileData> SearchAdjacentWall(TileData playerTile)
    {
        List<TileData> tilesInRange = NewManager.instance.CalculateReachableGrids(playerTile, range, false);
        List<TileData> wallsInRange = new List<TileData>();

        foreach (TileData tile in tilesInRange)
        {
            if (tile.myEntity != null && tile.myEntity.CompareTag("Wall"))
                wallsInRange.Add(tile);
        }

        adjacentTilesWithWalls = wallsInRange;
        return wallsInRange;
    }

#endregion

#region Play Effect

    IEnumerator ResolveList(string divide)
    {
        StringAndMethod dic = new StringAndMethod(this);
        divide = divide.Replace(" ", "");
        divide = divide.ToUpper();
        string[] methodsInStrings = divide.Split('/');

        foreach(string nextMethod in methodsInStrings)
        {
            debugger = 0;
            if (nextMethod == "" || nextMethod == "NONE")
            {
                continue;
            }
            else if (dic.dictionary.TryGetValue(nextMethod, out IEnumerator method))
            {
                yield return method;
                if (debugger == 0)
                    Debug.Log($"{this.name} failed to run");
            }
            else
            {
                Debug.LogError($"\"{nextMethod}\" for {this.name} isn't in the dictionary");
            }
        }
    }

    public IEnumerator OnPlayEffect()
    {
        yield return ResolveList(effectsInOrder);
    }

    public IEnumerator NextRoundEffect()
    {
        yield return ResolveList(nextRoundEffectsInOrder);
    }

    public IEnumerator DrawCards()
    {
        NewManager.instance.DrawCards(changeInDraw);
        debugger++;
        yield return null;
    }

    public Card FindCardType(CardType type)
    {
        List<Card> invalidCards = new List<Card>();
        Card foundCard = null;
        while (foundCard == null)
        {
            Card nextCard = NewManager.instance.GetTopCard();
            if (nextCard == null)
            {
                break;
            }
            else if (nextCard.typeOne == type || nextCard.typeTwo == type)
            {
                foundCard = nextCard;
            }
            else
            {
                invalidCards.Add(nextCard);
                nextCard.transform.SetParent(null);
            }
        }
        for (int i = 0; i < invalidCards.Count; i++)
            NewManager.instance.DiscardCard(invalidCards[i]);
        return foundCard;
    }

    public Card FindCardCost(int cost)
    {
        List<Card> invalidCards = new List<Card>();
        Card foundCard = null;
        while (foundCard == null)
        {
            Card nextCard = NewManager.instance.GetTopCard();
            if (nextCard == null)
            {
                break;
            }
            else if (nextCard.energyCost == cost)
            {
                foundCard = nextCard;
            }
            else
            {
                invalidCards.Add(nextCard);
                nextCard.transform.SetParent(null);
            }
        }
        for (int i = 0; i < invalidCards.Count; i++)
            NewManager.instance.DiscardCard(invalidCards[i]);

        return foundCard;
    }

    public IEnumerator DiscardHand()
    {
        debugger++;
        while (NewManager.instance.listOfHand.Count>0)
        {
            yield return NewManager.Wait(0.1f);
            NewManager.instance.DiscardCard(NewManager.instance.listOfHand[0]);
        }
    }

    public IEnumerator FindOne()
    {
        debugger++;
        for (int i = 0; i < 2; i++)
        {
            yield return NewManager.Wait(0.1f);
            NewManager.instance.AddCardToHand(FindCardCost(1));
        }
    }

    public IEnumerator ChangeHealth()
    {
        debugger++;
        NewManager.instance.ChangeHealth(changeInHP);
        yield return null;
    }

    public IEnumerator ChangeEnergy()
    {
        debugger++;
        NewManager.instance.ChangeEnergy(changeInEP);
        yield return null;
    }

    public IEnumerator ChangeMovement()
    {
        debugger++;
        NewManager.instance.ChangeMovement(changeInMP);
        yield return null;
    }

    public IEnumerator AffectAdjacentWall()
    {
        WallEntity targetWall = null;

        if (adjacentTilesWithWalls.Count == 1)
        {
            targetWall = adjacentTilesWithWalls[0].myEntity.GetComponent<WallEntity>();
        }
        else
        {
            NewManager.instance.UpdateInstructions("Choose a wall in range.");
            ChoiceManager.instance.ChooseTile(adjacentTilesWithWalls);
            while (ChoiceManager.instance.chosenTile == null)
                yield return null;
            targetWall = ChoiceManager.instance.chosenTile.myEntity.GetComponent<WallEntity>();
        }

        targetWall.AffectWall(changeInWall);
        debugger++;
    }

    public IEnumerator StunAdjacentGuard()
    {
        GuardEntity targetGuard = null;

        if (adjacentTilesWithWalls.Count == 1)
        {
            targetGuard = adjacentTilesWithGuards[0].myEntity.GetComponent<GuardEntity>();
        }
        else
        {
            NewManager.instance.UpdateInstructions("Choose a guard in range.");
            ChoiceManager.instance.ChooseTile(adjacentTilesWithGuards);
            while (ChoiceManager.instance.chosenTile == null)
                yield return null;
            targetGuard = ChoiceManager.instance.chosenTile.myEntity.GetComponent<GuardEntity>();
        }

        SoundManager.instance.PlaySound(targetGuard.stunSound);
        targetGuard.stunned += stunDuration;
        debugger++;
    }

#endregion
}
