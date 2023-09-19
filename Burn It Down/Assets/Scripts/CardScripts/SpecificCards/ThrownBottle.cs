using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownBottle : Card
{
    public override void Setup()
    {
        this.name = "Thrown Bottle";
        textName.text = "Thrown Bottle";
        energyCost = 3;
        textCost.text = $"{energyCost} Energy";
        textDescr.text = "Either make a guard look in your direction, or knock them out.";
        thisType = CardType.Violent;
    }

}
