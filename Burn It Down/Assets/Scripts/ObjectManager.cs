using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public Vector2Int CurrentGrid;

    //movement speed is the max distance an object can move on its turn
    [SerializeField] public int movementSpeed = 3;
    //movement points are the remaining points an object has to spend on moving
    public float movementPoints = 0;
    [SerializeField] float movePauseTime = 0.5f;

    public bool patrol = true;
    public GridManager manager;
    //direction determins the way they'll patrol and the tiles that are highlighted
    //south east = (-1,0),south west = (0,-1),north east = (1,0), North west = (0,1) 
    [SerializeField] Vector2Int direction = new Vector2Int(0,-1);

    [SerializeField] int DetectionRangePatrol = 3;

    [SerializeField] AudioClip noticeSound, moveSound;
    public int stunned = 0;
    public int hidden = 0;





    //called at end of a round or turn, only in effect for enemy tagged objects
    public void enemyEndTurn()
    {
        if (stunned == 0)
        {
            movementPoints = movementSpeed;
            StartCoroutine(guardPatrol(movePauseTime));
        }
        else
        {
            stunned--;
            manager.enemiesActive--;
        }

    }

    public void endPlayerTurn()
    {

        if (stunned != 0)
        {
            stunned--;
        }
        if (hidden != 0)
        {
            hidden--;
        }
        else
        {
            if (manager._Grid[CurrentGrid.x, CurrentGrid.y].underSurveillance)
            {
                TurnManager.instance.ChangeHealth((int)TurnManager.instance.healthBar.value - 1);
                SoundManager.instance.PlaySound(noticeSound);
            }
        }
    }

    IEnumerator guardPatrol(float pauseTimer)
    {
        float Timer = 0;
        while (Timer < pauseTimer)
        {
            Timer += Time.deltaTime;
            yield return null;
        }

        SoundManager.instance.PlaySound(moveSound);
        bool validSpace = true;
        bool trapped = false;
        //checking to see that the tile it wants to move onto is A) within the map and B) not a wall
        if (CurrentGrid.x + direction.x > 0 && CurrentGrid.x + direction.x < manager.GridSize.x + 1 &&
            CurrentGrid.y + direction.y > 0 && CurrentGrid.y + direction.y < manager.GridSize.y + 1)
        {
            if (manager._Grid[CurrentGrid.x + direction.x, CurrentGrid.y + direction.y].AttachedObject != null)
            {
                validSpace = false;
            }
        }
        else
        {
            validSpace = false;
        }


        if (validSpace)
        {
            manager._Grid[CurrentGrid.x, CurrentGrid.y].AttachedObject = null;
            CurrentGrid = CurrentGrid + direction;


            manager._Grid[CurrentGrid.x, CurrentGrid.y].AttachedObject = this;
            transform.parent = manager._Grid[CurrentGrid.x, CurrentGrid.y].transform;
            transform.position = new Vector3(CurrentGrid.x * manager.tileSize, manager._Grid[CurrentGrid.x, CurrentGrid.y].transform.position.y + manager.tileSize, CurrentGrid.y * -manager.tileSize);

            movementPoints--;
            print("moved! current point: " + movementPoints);
            if (movementPoints > 0)
            {
                StartCoroutine(guardPatrol(movePauseTime));
            }
            else
            {
                manager.enemiesActive--;
            }
        }
        else //if not a valid space ahead
        {
            //flips the guard 180 degrees
            direction = -direction;

            //checks to make sure the guard isn't trapped
            if (CurrentGrid.x + direction.x > 0 && CurrentGrid.x + direction.x < manager.GridSize.x &&
            CurrentGrid.y + direction.y > 0 && CurrentGrid.y + direction.y < manager.GridSize.y)
            {
                if (manager._Grid[CurrentGrid.x + direction.x, CurrentGrid.y + direction.y].AttachedObject != null)
                {
                    trapped = true;
                }
            }
            else
            {
                trapped = true;
            }

            if (trapped)
            {
                movementPoints = 0;
                manager.enemiesActive--;
            }
            else
            {
                manager._Grid[CurrentGrid.x, CurrentGrid.y].AttachedObject = null;
                CurrentGrid = CurrentGrid + direction;


                manager._Grid[CurrentGrid.x, CurrentGrid.y].AttachedObject = this;
                transform.parent = manager._Grid[CurrentGrid.x, CurrentGrid.y].transform;
                transform.position = new Vector3(CurrentGrid.x * manager.tileSize, manager._Grid[CurrentGrid.x, CurrentGrid.y].transform.position.y + manager.tileSize, CurrentGrid.y * -manager.tileSize);

                movementPoints--;
                if (movementPoints > 0)
                {
                    StartCoroutine(guardPatrol(movePauseTime));
                }
                else
                {
                    manager.enemiesActive--;
                }
            }
        }

    }

    private void Update()
    {
        if (gameObject.tag == "Enemy")
        {
            if (patrol && stunned == 0)
            {
                //creates a vector 2 to check to the left
                Vector2Int Side = Vector2Int.RoundToInt(Vector3.Cross((Vector2)direction, Vector3.forward));

                print("Side = " + Side);
                List<FloorTile> tilesToCheck = new List<FloorTile>()
                {
                    (manager._Grid[CurrentGrid.x + Side.x, CurrentGrid.y + Side.y]),
                    (manager._Grid[CurrentGrid.x + Side.x + direction.x, CurrentGrid.y + Side.y + direction.y]),
                    (manager._Grid[CurrentGrid.x - Side.x, CurrentGrid.y - Side.y]),
                    (manager._Grid[CurrentGrid.x - Side.x + direction.x, CurrentGrid.y - Side.y + direction.y])
                };
                foreach(FloorTile Tile in tilesToCheck)
                {
                    if (Tile.AttachedObject != null)
                    {
                        if (Tile.AttachedObject.tag != "Environmental")
                        {
                            Tile.underSurveillance = true;
                        }
                    }
                    else
                    {
                        Tile.underSurveillance = true;
                    }
                }


                for (int i = 1; i <= DetectionRangePatrol; i++)
                {
                    FloorTile targetGrid = manager._Grid[CurrentGrid.x + (direction.x * i), CurrentGrid.y + (direction.y * i)];
                    if (targetGrid.AttachedObject != null)
                    {
                        if (targetGrid.AttachedObject.tag != "Environmental")
                        {
                            targetGrid.underSurveillance = true;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        targetGrid.underSurveillance = true;
                    }
                }
            }
        }
    }

}
