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
        textDescr.text = "Gain +3 movement this turn.";
        thisType = CardType.NonViolent;
    }

    public override bool CanPlay()
    {
        return TurnManager.instance.energyBar.value >= energyCost;
    }

    public override IEnumerator PlayEffect()
    {
        GridManager.instance.Player1.movementPoints += 3;
        yield return null;
    }
}
