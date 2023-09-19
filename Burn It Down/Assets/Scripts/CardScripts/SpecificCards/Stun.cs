using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stun : Card
{
    public override void Setup()
    {
        this.name = "Stun";
        textName.text = "Stun";
        energyCost = 1;
        textCost.text = $"{energyCost} Energy";
        textDescr.text = "Knock out an adjacent guard for 1 turn.";
        thisType = CardType.Violent;
    }
}
