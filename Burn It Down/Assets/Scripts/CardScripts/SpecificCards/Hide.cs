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
        textDescr.text = "Guards won't spot you this turn.";
        thisType = CardType.NonViolent;
    }

    public override bool CanPlay()
    {
        return base.CanPlay();
    }

    public override IEnumerator PlayEffect()
    {
        return base.PlayEffect();
    }

}
