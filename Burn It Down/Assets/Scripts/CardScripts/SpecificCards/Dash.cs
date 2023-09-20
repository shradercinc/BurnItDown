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
        textDescr.text = "You can move 3 more spots this round.";
        thisType = CardType.NonViolent;
    }

    public override IEnumerator PlayEffect()
    {
        GridManager.instance.Player1.movementPoints += 3;
        yield return null;
    }
}
