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
        dictionary["DrawCards"] = card.DrawCards();
        dictionary["ChangeHP"] = card.ChangeHealth();
        dictionary["ChangeEP"] = card.ChangeEnergy();
        dictionary["ChangeMP"] = card.ChangeMovement();
    }

}

public class Card : MonoBehaviour, IPointerClickHandler
{
    [ReadOnly] public Image image;
    [ReadOnly] public SendChoice choiceScript;

    [ReadOnly] public int energyCost;
    public enum CardType { Attack, Draw, Distraction, Energy, Movement, Misc, None };
    [ReadOnly] public CardType typeOne;
    [ReadOnly] public CardType typeTwo;
    [ReadOnly] public bool violent;

    [ReadOnly] public int changeInHP;
    [ReadOnly] public int changeInMP;
    [ReadOnly] public int changeInEP;
    [ReadOnly] public int changeInDraw;

    [ReadOnly] public int stunDuration;
    [ReadOnly] public int range;
    [ReadOnly] public int areaOfEffect;
    [ReadOnly] public int delay;
    [ReadOnly] public int changeInWall;
    [ReadOnly] public int burnDuration;
    [ReadOnly] public int distractionIntensity;

    public enum CanPlayCondition { None, Guard, Wall, Occupied };
    [ReadOnly] public CanPlayCondition selectCondition;
    [ReadOnly] public List<IEnumerator> effectsInorder = new List<IEnumerator>();
    [ReadOnly] public string nextTurnAbility;

    [ReadOnly] public TMP_Text textName;
    [ReadOnly] public TMP_Text textCost;
    [ReadOnly] public TMP_Text textDescr;

    [ReadOnly] PlayerEntity currentPlayer;

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
}
