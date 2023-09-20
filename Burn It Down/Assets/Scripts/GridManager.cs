using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    //controls the grid size with x,y length (1-10 instead of 0-10)
    [SerializeField] public Vector2Int GridSize = new Vector2Int(10, 10);
    //grid of all floor tiles for quick reference
    public FloorTile[,] _Grid;
    //controls the spacing between each tile
    [SerializeField] public float tileSize = 2;
    //controls how high up the floor tiles are set
    [SerializeField] float baseTileLayer = 0;

    [Space(5)]

    [Header("Summonable Objects")]
    //these are generic for hard coading levels, tile is used for the flooring
    [SerializeField] GameObject genericTile;
    [SerializeField] GameObject genericWall;
    [SerializeField] GameObject genericGuard;
    [SerializeField] GameObject Player;

    [Space(5)]

    [Header("Selection Settings")]
    //checks currently selected tile
    public Vector2Int selectTile = new Vector2Int(0, 0);
    //quick refrence for the object within select tile
    public ObjectManager selectObject;


    [Header("Turn Management")]
    //in turn, 1 = player, 2 = Enemy, not a bool incase we need more states
    public int Turn = 1;
    //checks to make sure enemies arn't still taking their turn before swapping back to player control
    public float enemiesActive = 0;



    void Start()
    {
        //creates the base grid, note that all 0 slots are empty to make it easier to reference
        _Grid = new FloorTile[GridSize.x + 1, GridSize.y + 1];

        //generates the base grid tiles, adds them to an array that refrences the grid with an x/y position, technically 0,y and x,0 are valid, but nothing is held in them, 0,0 is used as "unselected space"
        for (int i = 1; i <= GridSize.x; i++)
        {
            for (int j = 1; j <= GridSize.y; j++)
            {
                GameObject curTile = Instantiate(genericTile, new Vector3(i * tileSize, baseTileLayer, j * -tileSize), Quaternion.identity);
                FloorTile curFloorScript = curTile.GetComponent<FloorTile>();
                curFloorScript.gridPosition = new Vector2Int(i, j);
                curFloorScript.manager = this;
                _Grid[i, j] = curFloorScript;
            }
        }


        //Generates objects onto the grid using i,j positions, assigns each object a manager and parents them to a tile
        for (int i = 1; i <= GridSize.x; i++)
        {
            for (int j = 1; j <= GridSize.y; j++)
            {
                //honestly these generation scripts are all the same and could be made into a single void

                //generates walls
                if (i == 6 && j <= 5)
                {
                    GameObject curObj = Instantiate(genericWall, new Vector3(_Grid[i, j].gridPosition.x * tileSize, baseTileLayer + tileSize, _Grid[i, j].gridPosition.y * -tileSize), Quaternion.identity);
                    ObjectManager curObjManager = curObj.GetComponent<ObjectManager>();
                    curObj.transform.parent = _Grid[i, j].transform;
                    _Grid[i, j].AttachedObject = curObjManager;
                    curObjManager.CurrentGrid = _Grid[i, j].gridPosition;
                    curObjManager.manager = this;
                }

                //generates guards
                if (i == 10 && j == 3)
                {
                    GameObject curObj = Instantiate(genericGuard, new Vector3(_Grid[i, j].gridPosition.x * tileSize, baseTileLayer + tileSize, _Grid[i, j].gridPosition.y * -tileSize), Quaternion.identity);
                    ObjectManager curObjManager = curObj.GetComponent<ObjectManager>();
                    curObj.transform.parent = _Grid[i, j].transform;
                    _Grid[i, j].AttachedObject = curObjManager;
                    curObjManager.CurrentGrid = _Grid[i, j].gridPosition;
                    curObjManager.manager = this;
                }

                //Generates the Player
                if (i == 1 && j == 1)
                {
                    GameObject curObj = Instantiate(Player, new Vector3(_Grid[i, j].gridPosition.x * tileSize, baseTileLayer + tileSize, _Grid[i, j].gridPosition.y * -tileSize), Quaternion.identity);
                    ObjectManager curObjManager = curObj.GetComponent<ObjectManager>();
                    curObj.transform.parent = _Grid[i, j].transform;
                    _Grid[i, j].AttachedObject = curObjManager;
                    curObjManager.CurrentGrid = _Grid[i, j].gridPosition;
                    curObjManager.manager = this;
                }
            }
        }

    }

    private void Update()
    {
        //checks to make sure all the enemies are done before switching back to player control
        if (Turn == 2)
        {
            if (enemiesActive == 0)
            {
                Turn = 1;
            }
        }
    }

    public void endTurn()
    {
        //sets turn to the enemies, and counts through the grid activating all enemies simultaniously
        Turn = 2;
        for (int i = 1; i <= GridSize.x; i++)
        {
            for (int j = 1; j <= GridSize.y; j++)
            {
                if (_Grid[i, j].AttachedObject != null)
                {
                    if (_Grid[i, j].AttachedObject.gameObject.tag == "Enemy")
                    {
                        enemiesActive++;
                        _Grid[i, j].AttachedObject.enemyEndTurn();
                    }
                }
            }
        }
    }

    public void endRound()
    {
        //sets turn to the enemies, and counts through the grid activating all enemies simultaniously
        Turn = 2;
        for (int i = 1; i <= GridSize.x; i++)
        {
            for (int j = 1; j <= GridSize.y; j++)
            {
                if (_Grid[i, j].AttachedObject != null)
                {
                    if (_Grid[i, j].AttachedObject.gameObject.tag == "Player")
                    {
                        _Grid[i, j].AttachedObject.movementPoints = _Grid[i, j].AttachedObject.movementSpeed;

                    }
                    if (_Grid[i, j].AttachedObject.gameObject.tag == "Enemy")
                    {
                        enemiesActive++;
                        _Grid[i, j].AttachedObject.enemyEndTurn();
                    }
                }
            }
        }
    }

}
