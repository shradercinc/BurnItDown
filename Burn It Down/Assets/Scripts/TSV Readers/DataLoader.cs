using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CardData
{

}

public static class CardDataLoader
{
    public static List<CardData> ReadCardData()
    {
        List<CardData> cardData = new List<CardData>();
        var data = TSVReader.Read("CardData", 2);

        return cardData;
    }
}
