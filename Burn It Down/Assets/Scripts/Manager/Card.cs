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
        dictionary["FINDZERO"] = card.FindZero();
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
    [ReadOnly] List<IEnumerator> effectsInorder = new List<IEnumerator>();
    [ReadOnly] List<IEnumerator> nextRoundEffectsInOrder = new List<IEnumerator>();

    [ReadOnly] public TMP_Text textName { get; private set; } 
    [ReadOnly] public TMP_Text textCost { get; private set; }
    [ReadOnly] public TMP_Text textDescr { get; private set; }

    [ReadOnly] PlayerEntity currentPlayer;
    [ReadOnly] List<TileData> adjacentTilesWithGuards = new List<TileData>();
    [ReadOnly] List<TileData> adjacentTilesWithWalls = new List<TileData>();

    public AudioClip cardMove;
    public AudioClip cardPlay;
    

#endregion

#region Setup

    private void Awake()
    {
        image = GetComponent<Image>();
        choiceScript = GetComponent<SendChoice>();

        textName = this.transform.GetChild(1).GetComponent<TMP_Text>();
        textCost = this.transform.GetChild(2).GetComponent<TMP_Text>();
        textDescr = this.transform.GetChild(3).GetComponent<TMP_Text>();
    }

    private void Start()
    {
        SaveManager.instance.allCards.Add(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("right clicked");
            RightClick.instance.ChangeCard(this);
            SoundManager.instance.PlaySound(cardMove);
        }
    }

    public void CardSetup(CardData data)
    {
        name = data.name;
        textName.text = data.name;
        textDescr.text = data.desc;

        typeOne = ConvertToType(data.cat1);
        typeTwo = ConvertToType(data.cat2);

        energyCost = data.epCost;
        textCost.text = $"{data.epCost}";
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
        AddMethodsToList(data.action, effectsInorder);
        AddMethodsToList(data.nextAct, nextRoundEffectsInOrder);
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

    void AddMethodsToList(string divide, List<IEnumerator> list)
    {
        StringAndMethod dic = new StringAndMethod(this);
        divide = divide.Replace(" ", "");
        divide = divide.ToUpper();
        string[] methodsInStrings = divide.Split('/');

        for (int k = 0; k < methodsInStrings.Length; k++)
        {
            if (methodsInStrings[k] == "" || methodsInStrings[k] == "NONE")
                continue;
            else if (dic.dictionary.TryGetValue(methodsInStrings[k], out IEnumerator method))
                list.Add(method);
            else
                Debug.LogError($"\"{methodsInStrings[k]}\" for {this.name} isn't in the dictionary");
        }
    }

#endregion

#region Play Condition

    public bool CanPlay(PlayerEntity player)
    {
        currentPlayer = player;
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
        else
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

    public IEnumerator OnPlayEffect()
    {
        for (int i = 0; i < effectsInorder.Count; i++)
            yield return effectsInorder[i];
    }

    public IEnumerator NextRoundEffect()
    {
        for (int i = 0; i < nextRoundEffectsInOrder.Count; i++)
            yield return nextRoundEffectsInOrder[i];
    }

    public IEnumerator DrawCards()
    {
        NewManager.instance.DrawCards(changeInDraw);
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
        while (NewManager.instance.listOfHand.Count>0)
        {
            yield return NewManager.Wait(0.25f);
            NewManager.instance.DiscardCard(NewManager.instance.listOfHand[0]);
        }
    }

    public IEnumerator FindZero()
    {
        for (int i = 0; i < 2; i++)
        {
            yield return NewManager.Wait(0.25f);
            NewManager.instance.AddCardToHand(FindCardCost(0));
        }
    }

    public IEnumerator ChangeHealth()
    {
        NewManager.instance.ChangeHealth(changeInHP);
        yield return null;
    }

    public IEnumerator ChangeEnergy()
    {
        NewManager.instance.ChangeEnergy(changeInEP);
        yield return null;
    }

    public IEnumerator ChangeMovement()
    {
        NewManager.instance.ChangeMovement(changeInMP);
        yield return null;
    }

    public IEnumerator AffectAdjacentWall()
    {
        Debug.Log($"there are {adjacentTilesWithWalls.Count} walls");
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
    }

    public IEnumerator StunAdjacentGuard()
    {
        Debug.Log($"there are {adjacentTilesWithGuards.Count} guards");
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
        Debug.Log("Stunned for " + targetGuard.stunned);
    }

#endregion
}
