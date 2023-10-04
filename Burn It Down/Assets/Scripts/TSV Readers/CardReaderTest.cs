using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardReaderTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        List<CardData> data = CardDataLoader.ReadCardData();
        foreach (CardData card in data)
        {
            Debug.Log(card.name);
        }
    }
}
