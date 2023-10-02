using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hide : Card
{
    public override void Setup()
    {
        this.name = "Hide";
        textName.text = "Hide";
        energyCost = 2;
        textCost.text = $"{energyCost} Energy";
        textDescr.text = "Guards won't spot you for 2 turns.";
        thisType = CardType.NonViolent;
    }

    public override bool CanPlay()
    {
        return NewManager.instance.EnoughEnergy(energyCost);
    }

    public override IEnumerator PlayEffect()
    {
        NewManager.instance.listOfPlayers[0].hidden += 2;
        yield return null;
    }

}