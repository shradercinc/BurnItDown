using Pathfinding.Util;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using MyBox;

public class Entity : MonoBehaviour
{
    [Foldout("Base Entity", true)]
        [Tooltip("Store this entity's position")] [ReadOnly] public TileData currentTile;
        [ReadOnly] public MeshRenderer meshRenderer;
        [ReadOnly] public LineRenderer lineRenderer;
        [Tooltip("Cost of moving through item, default 999 (intraversable)")][SerializeField] public int MoveCost = 999; 
    
    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    public virtual string HoverBoxName()
    {
        return "";
    }

    public virtual string HoverBoxText()
    {
        return "";
    }

    public void MoveTile(TileData newTile)
    {
        newTile = NewManager.instance.listOfTiles[newTile.gridPosition.x, newTile.gridPosition.y];
        print("In move " + newTile.gridPosition);
        if (currentTile != null)
            currentTile.myEntity = null;

        newTile.myEntity = this;
        this.currentTile = newTile;
        this.transform.SetParent(newTile.transform);
        this.transform.localScale = new Vector3(1, 1, 1);
        this.transform.localPosition = new Vector3(0, 1, 0);
        CalculateTiles();
    }

    public virtual void CalculateTiles()
    {
        
    }
}
