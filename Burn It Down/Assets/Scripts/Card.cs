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
    [ReadOnly] List<IEnumerator> nextTurnEffectsInOrder = new List<IEnumerator>();

    [ReadOnly] public TMP_Text textName;
    [ReadOnly] public TMP_Text textCost;
    [ReadOnly] public TMP_Text textDescr;

    [ReadOnly] PlayerEntity currentPlayer;

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
        textCost.text = $"{data.epCost} Energy";
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
        AddMethodsToList(data.action, nextTurnEffectsInOrder);
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
            if (methodsInStrings[k] != "" || methodsInStrings[k] != "NONE")
            {
                if (dic.dictionary.TryGetValue(methodsInStrings[k], out IEnumerator method))
                    list.Add(method);
                else
                    Debug.LogError($"\"{methodsInStrings[k]}\" isn't a method");
            }
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
                CanPlayCondition.Guard => SearchAdjacent(OccupiedAdjacent(player.currentTile), "Enemy"),
                CanPlayCondition.Wall => SearchAdjacent(OccupiedAdjacent(player.currentTile), "Wall"),
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
        List<TileData> adjacentOccupied = new List<TileData>();
        for (int i = 0; i<playerTile.adjacentTiles.Count; i++)
        {
            if (playerTile.adjacentTiles[i].myEntity != null)
                adjacentOccupied.Add(playerTile.adjacentTiles[i]);
        }
        return adjacentOccupied;
    }

    bool SearchAdjacent(List<TileData> adjacentOccupied, string tag)
    {
        for (int i = 0; i<adjacentOccupied.Count; i++)
        {
            if (adjacentOccupied[i].myEntity.CompareTag(tag))
                return true;
        }
        return false;
    }

    #endregion

#region Play Effect

    public IEnumerator OnPlayEffect()
    {
        for (int i = 0; i < effectsInorder.Count; i++)
            yield return effectsInorder[i];
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
            if (NewManager.instance.GetTopCard() == null)
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
            if (NewManager.instance.GetTopCard() == null)
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

#endregion
}
