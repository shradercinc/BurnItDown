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
        return NewManager.instance.EnoughEnergy(energyCost);
    }

    public override IEnumerator PlayEffect()
    {
        NewManager.instance.listOfPlayers[0].movementLeft += 3;
        yield return null;
    }
}
