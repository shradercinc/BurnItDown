using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stun : Card
{
    ObjectManager adjacentEnemy;

    public override void Setup()
    {
        this.name = "Stun";
        textName.text = "Stun";
        energyCost = 1;
        textCost.text = $"{energyCost} Energy";
        textDescr.text = "Knock out an adjacent guard for 3 turns.";
        thisType = CardType.Violent;
    }

    public override bool CanPlay()
    {
        if (TurnManager.instance.energyBar.value > energyCost)
        {
            Vector2Int currentTile = GridManager.instance.Player1.transform.parent.GetComponent<FloorTile>().gridPosition;
            FloorTile[] adjacent = new FloorTile[4];

            try
            {
                adjacent[0] = GridManager.instance._Grid[currentTile.x - 1, currentTile.y];
            }
            catch (System.IndexOutOfRangeException)
            {
                adjacent[0] = null;
            }
            try
            {
                adjacent[1] = GridManager.instance._Grid[currentTile.x + 1, currentTile.y];
            }
            catch (System.IndexOutOfRangeException)
            {
                adjacent[1] = null;
            }
            try
            {
                adjacent[2] = GridManager.instance._Grid[currentTile.x, currentTile.y - 1];
            }
            catch (System.IndexOutOfRangeException)
            {
                adjacent[2] = null;
            }
            try
            {
                adjacent[3] = GridManager.instance._Grid[currentTile.x, currentTile.y + 1];
            }
            catch (System.IndexOutOfRangeException)
            {
                adjacent[3] = null;
            }
            for (int i = 0; i < adjacent.Length; i++)
            {
                if (adjacent[i] != null)
                {
                    if (CheckForEnemy(adjacent[i]))
                    {
                        adjacentEnemy = adjacent[i].AttachedObject;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    bool CheckForEnemy(FloorTile nextTile)
    {
        if (nextTile == null || nextTile.AttachedObject == null)
            return false;
        Debug.Log("checking attached object");
        return nextTile.AttachedObject.CompareTag("Enemy");
    }

    public override IEnumerator PlayEffect()
    {
        adjacentEnemy.stunned += 3;
        SoundManager.instance.PlaySound(SoundManager.instance.stun);
        yield return null;
    }
}
