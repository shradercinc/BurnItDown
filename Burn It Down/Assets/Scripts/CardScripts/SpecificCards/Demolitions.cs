using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demolitions : Card
{
    GameObject adjacentWall;

    public override void Setup()
    {
        this.name = "Demolitions";
        textName.text = "Demolitions";
        energyCost = 1;
        textCost.text = $"{energyCost} Energy";
        textDescr.text = "Break down an adjacent wall.";
        thisType = CardType.NonViolent;
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
                    if (CheckForWall(adjacent[i]))
                    {
                        adjacentWall = adjacent[i].AttachedObject.gameObject;
                        return true;
                    }
                }
            }
            }
        return false;
    }

    bool CheckForWall(FloorTile nextTile)
    {
        if (nextTile == null || nextTile.AttachedObject == null)
            return false;
        Debug.Log("checking attached object");
        return nextTile.AttachedObject.CompareTag("Wall");
    }

    public override IEnumerator PlayEffect()
    {
        Destroy(adjacentWall);
        SoundManager.instance.PlaySound(SoundManager.instance.demolish);
        yield return null;
    }
}
