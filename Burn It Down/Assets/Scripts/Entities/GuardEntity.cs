using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using System.Security.Cryptography.X509Certificates;

public class GuardEntity : MovingEntity
{
    [Foldout("Guard Entity", true)]
        [Tooltip("Tiles this is searching")] List<TileData> inDetection = new List<TileData>();
        [Tooltip("Pauses between movement")] float movePauseTime = 0.25f;
        [Tooltip("How far this can see")][SerializeField] int DetectionRangePatrol = 3;
        [Tooltip("Turns which this does nothing")] [ReadOnly] public int stunned = 0;
        [Tooltip("Times this attacks")] [ReadOnly] public int attacksPerTurn = 1;
        [Tooltip("Current number of attacks")][ReadOnly] int attacksLeft = 0;
        [Tooltip("Current Target to attack & persue")] PlayerEntity CurrentTarget;
        [Tooltip("State of a guard's alert, 0 = patrol, 1 = attack")] int Alert = 0;
        [Tooltip("Guard Range")] int AttackRange = 1;
        [Tooltip("list of patrol positions")] List<Vector2Int> PatrolPoints = new List<Vector2Int>();

    public override string HoverBoxText()
    {
        string answer = "";
        if (stunned > 0)
            answer += $"Stunned for {stunned} turns\n";
        return answer;
    }

    public override void CalculateTiles()
    {
        for (int i = 0; i<inDetection.Count; i++)
            inDetection[i].SurveillanceState(false);
        inDetection.Clear();

        Vector2Int side = Vector2Int.RoundToInt(Vector3.Cross((Vector2)direction, Vector3.forward));
        for (int i = 0; i < DetectionRangePatrol; i++)
        {
            inDetection.Add(NewManager.instance.FindTile(currentTile.gridPosition + new Vector2Int(direction.x * i, direction.y * i)));
            if (i <= 1)
            {
                inDetection.Add(NewManager.instance.FindTile(currentTile.gridPosition + side + new Vector2Int(direction.x * i, direction.y * i)));
                inDetection.Add(NewManager.instance.FindTile(currentTile.gridPosition - side + new Vector2Int(direction.x * i, direction.y * i)));
            }
        }

        inDetection.RemoveAll(item => item == null); //delete all tiles that are null
        for (int i = 0; i < inDetection.Count; i++)
            inDetection[i].SurveillanceState(true);
    }

    /*
    public PlayerEntity CheckForPlayer()
    {
        for (int i = 0; i<inDetection.Count; i++)
        {
            print("guard: " + currentTile.gridPosition + " Looking at " + inDetection[i].gridPosition);
            if (inDetection[i].myEntity != null && inDetection[i].myEntity.CompareTag("Player"))

                if (inDetection[i].myEntity.GetComponent<PlayerEntity>().hidden !> 0)
                {
                    print("found player");
                    return inDetection[i].myEntity.GetComponent<PlayerEntity>();
                }
        }
        return null;
    }
    */

    public void CheckForPlayer()
    {
        for (int i = 0; i < inDetection.Count; i++)
        {
            //print("guard: " + currentTile.gridPosition + " Looking at " + inDetection[i].gridPosition);
            if (inDetection[i].myEntity != null)
            {
                if(inDetection[i].myEntity.CompareTag("Player"))
                {
                    if (inDetection[i].myEntity.GetComponent<PlayerEntity>().hidden == 0)
                    {
                        print("found player");
                        alerted(inDetection[i].myEntity.GetComponent<PlayerEntity>());
                    }
                }
            }
        }
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
            attacksLeft = attacksPerTurn;
            CheckForPlayer();
            if(Alert == 0)
                yield return Patrol();
            else
                yield return Attack(CurrentTarget);
        }
    }

    public void alerted(PlayerEntity target)
    {
        Alert = 1;
        CurrentTarget = target;
        print("New target, player at " + target.currentTile.gridPosition);
    }

    IEnumerator Attack(PlayerEntity detectedPlayer)
    {
        RaycastHit hit;
        Vector3 shotDirection = detectedPlayer.transform.position - transform.position;
        if (Physics.Raycast(transform.position, shotDirection, out hit, Mathf.Infinity, 1 << 2))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                if (NewManager.instance.GetDistance(currentTile, detectedPlayer.currentTile) > AttackRange && movementLeft > 0)
                {
                    NewManager.instance.CalculatePathfinding(currentTile, detectedPlayer.currentTile, movementLeft, true);
                    MoveTile(NewManager.instance.CurrentAvalibleMoveTarget);
                    movementLeft--;
                }
                if (NewManager.instance.GetDistance(currentTile, detectedPlayer.currentTile) <= AttackRange && attacksLeft > 0)
                {
                    NewManager.instance.ChangeHealth(-1);
                    attacksLeft--;
                }
            }
            else
            {
                Alert = 0;
                CurrentTarget = null;
            }
        }
        float timer = 0;
        while (timer < movePauseTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        if (Alert == 1)
        {

        }
        else if (Alert == 0)
        {
            
        }

    }

    IEnumerator Patrol()
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
        float timer = 0;
        while (timer < movePauseTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        this.MoveTile(nextTile); //move to the tile
        //timesMoved++;
        yield return new WaitForSeconds(movePauseTime);
    }
}
