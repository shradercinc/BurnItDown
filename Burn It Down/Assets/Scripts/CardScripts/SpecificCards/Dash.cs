using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : Card
{
    public override void Setup()
    {
        this.name = "Dash";
        textName.text = "Dash";
        energyCost = 0;
        textCost.text = $"{energyCost} Energy";
        textDescr.text = "When you move this turn, you can move 3 more spots.";
        thisType = CardType.NonViolent;
    }
}
