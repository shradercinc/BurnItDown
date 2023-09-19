using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whistle : Card
{
    public override void Setup()
    {
        this.name = "Whistle";
        textName.text = "Whistle";
        energyCost = 1;
        textCost.text = $"{energyCost} Energy";
        textDescr.text = "Make a guard look in your direction.";
        thisType = CardType.NonViolent;
    }
}
