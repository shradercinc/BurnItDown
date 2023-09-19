using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrassKnuckles : Card
{
    public override void Setup()
    {
        this.name = "Brass Knuckles";
        textName.text = "Brass Knuckles";
        energyCost = 2;
        textCost.text = $"{energyCost} Energy";
        textDescr.text = "Knock out an adjacent guard for 2 turns.";
        thisType = CardType.Violent;
    }

}
