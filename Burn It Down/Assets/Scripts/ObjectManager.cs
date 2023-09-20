using System.Collections;
using System.Collections.Generic;
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

    public bool patrol;
    public GridManager manager;
    //direction determins the way they'll patrol and the tiles that are highlighted
    //south east = (-1,0),south west = (0,-1),north east = (1,0), North west = (0,1) 
    [SerializeField] Vector2Int direction = new Vector2Int(-1,0);






    //called at end of a round or turn, only in effect for enemy tagged objects
    public void enemyEndTurn()
    {
        movementPoints = movementSpeed;
        StartCoroutine(guardPatrol(movePauseTime));
    }

    IEnumerator guardPatrol(float pauseTimer)
    {
        float Timer = 0;
        while (Timer < pauseTimer)
        {
            Timer += Time.deltaTime;
            yield return null;
        }


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
            
        }
    }

}
