using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class GuardEntity : MovingEntity
{
    [Foldout("Guard Entity", true)]
        [Tooltip("Tiles this is searching")] List<TileData> inDetection = new List<TileData>();
        [Tooltip("Pauses between movement")][SerializeField] float movePauseTime = 0.25f;
        [Tooltip("How far this can see")][SerializeField] int DetectionRangePatrol = 3;
        [Tooltip("Turns which this does nothing")]public int stunned = 0;
        [Tooltip("Times this attacks")] public int attacksPerTurn = 1;
        [Tooltip("Where this moves and looks")] public Vector2Int direction;

    public override void CalculateTiles()
    {
        for (int i = 0; i<inDetection.Count; i++)
            inDetection[i].SurveillanceState(false);
        inDetection.Clear();

        Vector2Int side = Vector2Int.RoundToInt(Vector3.Cross((Vector2)direction, Vector3.forward));
        for (int i = 0; i < DetectionRangePatrol; i++)
        {
            inDetection.Add(NewManager.instance.FindTile(currentTile.gridPosition + new Vector2Int(direction.x * i, direction.y * i)));
            inDetection.Add(NewManager.instance.FindTile(currentTile.gridPosition + side + new Vector2Int(direction.x * i, direction.y * i)));
            inDetection.Add(NewManager.instance.FindTile(currentTile.gridPosition - side + new Vector2Int(direction.x * i, direction.y * i)));
        }

        inDetection.RemoveAll(item => item == null); //delete all tiles that are null
        for (int i = 0; i < inDetection.Count; i++)
            inDetection[i].SurveillanceState(true);
    }

    PlayerEntity CheckForPlayer()
    {
        for (int i = 0; i<inDetection.Count; i++)
        {
            if (inDetection[i].myEntity.CompareTag("Player"))
                return inDetection[i].myEntity.GetComponent<PlayerEntity>();
        }
        return null;
    }

    public override IEnumerator EndOfTurn()
    {
        if (stunned > 0)
        {
            stunned--;
        }
        else
        {
            movementLeft = movesPerTurn;
            PlayerEntity detectedPlayer = CheckForPlayer();
            if (detectedPlayer == null)
                yield return Patrol();
            else
                yield return Attack(detectedPlayer);
        }
    }

    IEnumerator Attack(PlayerEntity detectedPlayer)
    {
        yield return new WaitForSeconds(movePauseTime);
        NewManager.instance.ChangeHealth(-1);
    }

    IEnumerator Patrol()
    {
        int timesMoved = 0;
        while (timesMoved < movementLeft)
        {
            TileData nextTile;
            nextTile = NewManager.instance.FindTile(currentTile.gridPosition + direction); //find tile in the current direction
            while (nextTile == null || nextTile.myEntity != null) //if it can't
            {
                List<TileData> possibleTiles = new List<TileData>();
                for (int i = 0; i<currentTile.adjacentTiles.Count; i++)
                {
                    if (currentTile.adjacentTiles[i].myEntity == null) //find all adjacent tiles that this can move to
                        possibleTiles.Add(currentTile.adjacentTiles[i]);
                }
                nextTile = possibleTiles[Random.Range(0, possibleTiles.Count)]; //pick a random tile that's available
                direction = nextTile.gridPosition - currentTile.gridPosition; //change direction
            }

            this.MoveTile(nextTile); //move to the tile
            timesMoved++;
            yield return new WaitForSeconds(movePauseTime);
        }
    }
}
