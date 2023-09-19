using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burn : Card
{
    public override void Setup()
    {
        this.name = "Burn";
        textName.text = "Burn";
        energyCost = 1;
        textCost.text = $"{energyCost} Energy";
        textDescr.text = "Burn down an adjacent object.";
        thisType = CardType.NonViolent;
    }
}
