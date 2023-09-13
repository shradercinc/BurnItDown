using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] Vector2Int GridSize = new Vector2Int(10,10);
    private static int[,] _Grid;
    [SerializeField] float tileSize = 2;
    [SerializeField] GameObject genericTile;
    [SerializeField] float baseTileLayer = 0;
    public Vector2Int selectTile = new Vector2Int(-1,-1);
    public List<GameObject> gridList = new List<GameObject>();
    public List<FloorTile> FloorList = new List<FloorTile>();
    // Start is called before the first frame update
    void Start()
    {
        _Grid = new int[GridSize.x, GridSize.y];
        for(int i = 0; i < GridSize.x; i++)
        {
            for (int j = 0; j < GridSize.y; j++)
            {
                GameObject curTile = Instantiate(genericTile, new Vector3(i * tileSize,baseTileLayer,j * -tileSize) ,Quaternion.identity);
                FloorTile curFloorScript = curTile.GetComponent<FloorTile>();
                curFloorScript.gridPosition = new Vector2Int(i, j);
                curFloorScript.manager = this;
                gridList.Add(curTile);
                FloorList.Add(curFloorScript);
            }
        }
        for (int i = 0; i < gridList.Count; i++)
        {
           if() 
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
