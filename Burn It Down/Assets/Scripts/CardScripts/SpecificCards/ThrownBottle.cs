using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownBottle : Card
{
    public override void Setup()
    {
        this.name = "Thrown Bottle";
        textName.text = "Thrown Bottle";
        energyCost = 2;
        textCost.text = $"{energyCost} Energy";
        textDescr.text = "Knock out a guard from any distance for 2 turns.";
        thisType = CardType.Violent;
    }

    public override bool CanPlay()
    {
        return TurnManager.instance.energyBar.value >= energyCost;
    }

    public override IEnumerator PlayEffect()
    {
        GameObject.Find("GenericGuard(Clone)").GetComponent<ObjectManager>().stunned += 2;
        yield return null;
    }

}
