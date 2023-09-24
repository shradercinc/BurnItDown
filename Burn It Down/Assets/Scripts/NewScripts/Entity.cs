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
        [Tooltip("Store this entity's position")]public TileData currentTile;
        MeshRenderer meshRenderer;
        public LineRenderer lineRenderer;
    
    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
    }

    public void MoveTile(TileData newTile)
    {
        if (currentTile != null)
            currentTile.myEntity = null;

        newTile.myEntity = this;
        this.currentTile = newTile;
        this.transform.SetParent(newTile.transform);
        this.transform.localPosition = new Vector3(0, 0, 0);
        CalculateTiles();
    }

    public virtual void CalculateTiles()
    {
        
    }
}
