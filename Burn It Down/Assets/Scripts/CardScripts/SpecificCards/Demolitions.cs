using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demolitions : Card
{
    public override void Setup()
    {
        this.name = "Demolitions";
        textName.text = "Demolitions";
        energyCost = 1;
        textCost.text = $"{energyCost} Energy";
        textDescr.text = "Break down an adjacent wall.";
        thisType = CardType.NonViolent;
    }
}
