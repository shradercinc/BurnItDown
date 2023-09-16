using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] Vector2Int GridSize = new Vector2Int(10,10);
    public FloorTile[,] _Grid;
    [SerializeField] public float tileSize = 2;
    [SerializeField] float baseTileLayer = 0;
    
    [Space(5)]

    [Header("Summonable Objects")]
    [SerializeField] GameObject genericTile;
    [SerializeField] GameObject genericWall;
    [SerializeField] GameObject genericGuard;
    [SerializeField] GameObject Player;

    [Space(5)]

    [Header("Selection Settings")]
    public Vector2Int selectTile = new Vector2Int(0, 0);
    public ObjectManager selectObject;



    void Start()
    {
        _Grid = new FloorTile[GridSize.x + 1, GridSize.y + 1];
        
        //generates the base grid tiles, adds them to an array that refrences the grid with an x/y position, technically 0,y and x,0 are valid, but nothing is held in them, 0,0 is used as "unselected space"
        for(int i = 1; i <= GridSize.x; i++)
        {
            for (int j = 1; j <= GridSize.y; j++)
            {
                GameObject curTile = Instantiate(genericTile, new Vector3(i * tileSize,baseTileLayer,j * -tileSize) ,Quaternion.identity);
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
                if (i == 6 && j <= 5)
                {
                    GameObject curObj = Instantiate(genericWall, new Vector3(_Grid[i, j].gridPosition.x * tileSize, baseTileLayer + tileSize, _Grid[i, j].gridPosition.y * -tileSize), Quaternion.identity);
                    ObjectManager curObjManager = curObj.GetComponent<ObjectManager>();
                    curObj.transform.parent = _Grid[i,j].transform;
                }

                if (i == 10 && j == 3)
                {
                    GameObject curObj = Instantiate(genericGuard, new Vector3(_Grid[i, j].gridPosition.x * tileSize, baseTileLayer + tileSize, _Grid[i, j].gridPosition.y * -tileSize), Quaternion.identity);
                    ObjectManager curObjManager = curObj.GetComponent<ObjectManager>();
                    curObj.transform.parent = _Grid[i, j].transform;
                }

                if (i == 1 && j == 1)
                {
                    GameObject curObj = Instantiate(Player, new Vector3(_Grid[i, j].gridPosition.x * tileSize, baseTileLayer + tileSize, _Grid[i, j].gridPosition.y * -tileSize), Quaternion.identity);
                    ObjectManager curObjManager = curObj.GetComponent<ObjectManager>();
                    curObj.transform.parent = _Grid[i, j].transform;
                    _Grid[i, j].AttachedObject = curObjManager;
                    curObjManager.CurrentGrid = _Grid[i, j].gridPosition;
                }
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
