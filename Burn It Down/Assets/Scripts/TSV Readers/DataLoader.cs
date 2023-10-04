using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//the data that a single card holds; this has the same fields and descriptions as in the spreadsheet
public class CardData
{
    public string name;    //name at top of card
    public string desc;    //card explanation displayed on the card itself
    public string cat1;    //atk (attack), draw, dist (distraction), eng (energy), mvmt (movement), misc (miscellaneous)
    public string cat2;    //same categories as above

    public int maxInv;     //number of cards in inventory
    public int epCost;     //EP cost to play card
    public bool isViolent; //violent or nonviolent

    public int chHP;       //HP change
    public int chMP;       //MP change
    public int chEP;       //EP change
    public int draw;       //number of cards to draw

    public int stun;       //number of turns the action stuns
    public int range;      //range of action
    public int aoe;        //area of action effect
    public int delay;      //number of turns to delay
    public int wHP;        //change in wall HP
    public int burn;       //amount to burn
    public int intn;       //distraction intensity

    public string select;  //any extra condition needed to select a tile
    public string action;  //action to do
    public string nextAct; //action to do next turn
}

//Uses TSVReader class to translate values from the CardData CSV.
public class CardDataLoader
{
    //Call this function at the beginning of your script to generate the cards and deck
    public static List<CardData> ReadCardData()
    {
        List<CardData> cardData = new List<CardData>();
        var data = TSVReader.Read("CardData", 2);
        foreach (string[] line in data)
        {
            CardData newCard = new CardData();
            cardData.Add(newCard);

            newCard.name =   line[0];
            newCard.desc =   line[1];
            newCard.cat1 =   line[2];
            newCard.cat2 =   line[3];
            newCard.maxInv = int.Parse(line[4]);
            newCard.epCost = int.Parse(line[5]);
            newCard.isViolent = line[6] == "v";
            newCard.chHP = int.Parse(line[7]);
            newCard.chMP = int.Parse(line[8]);
            newCard.chEP = int.Parse(line[9]);
            newCard.draw = int.Parse(line[10]);
            newCard.stun = int.Parse(line[11]);
            newCard.range = int.Parse(line[12]);
            newCard.aoe = int.Parse(line[13]);
            newCard.delay = int.Parse(line[14]);
            newCard.wHP = int.Parse(line[15]);
            newCard.burn = int.Parse(line[16]);
            newCard.intn = int.Parse(line[17]);
            newCard.select = line[18];
            newCard.action = line[19];
            newCard.nextAct = line[20];
        }
        return cardData;
    }
}
