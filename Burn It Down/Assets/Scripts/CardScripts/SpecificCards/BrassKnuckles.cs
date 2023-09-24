using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrassKnuckles : Card
{
    GuardEntity adjacentEnemy;

    public override void Setup()
    {
        this.name = "Brass Knuckles";
        textName.text = "Brass Knuckles";
        energyCost = 2;
        textCost.text = $"{energyCost} Energy";
        textDescr.text = "Knock out an adjacent guard for 5 turns.";
        thisType = CardType.Violent;
    }

    public override bool CanPlay()
    {
        if (NewManager.instance.EnoughEnergy(energyCost))
        {
            Vector2Int currentTile = NewManager.instance.listOfPlayers[0].currentTile.gridPosition;
            TileData[] adjacent = new TileData[4];

            try
            {
                adjacent[0] = NewManager.instance.tilesInGrid[currentTile.x - 1, currentTile.y];
            }
            catch (System.IndexOutOfRangeException)
            {
                adjacent[0] = null;
            }
            try
            {
                adjacent[1] = NewManager.instance.tilesInGrid[currentTile.x + 1, currentTile.y];
            }
            catch (System.IndexOutOfRangeException)
            {
                adjacent[1] = null;
            }
            try
            {
                adjacent[2] = NewManager.instance.tilesInGrid[currentTile.x, currentTile.y - 1];
            }
            catch (System.IndexOutOfRangeException)
            {
                adjacent[2] = null;
            }
            try
            {
                adjacent[3] = NewManager.instance.tilesInGrid[currentTile.x, currentTile.y + 1];
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
                        adjacentEnemy = adjacent[i].myEntity.GetComponent<GuardEntity>();
                        return true;
                    }
                }
            }
        }
        return false;
    }

    bool CheckForEnemy(TileData nextTile)
    {
        if (nextTile == null || nextTile.myEntity == null)
            return false;
        Debug.Log("checking attached object");
        return nextTile.myEntity.CompareTag("Enemy");
    }

    public override IEnumerator PlayEffect()
    {
        adjacentEnemy.stunned += 5;
        yield return null;
    }
}
