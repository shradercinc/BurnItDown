using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FloorTile : MonoBehaviour
{
    public Vector2Int gridPosition;
    [SerializeField] float hoverDistance;
    [SerializeField] float climbSpeed = 0.9f;
    [SerializeField] float dropSpeed;
    private bool hover = false;
    private float baseHeight = 0;
    public GridManager manager;
    // Start is called before the first frame update

    void Start()
    {
    }

    private void OnMouseOver()
    {
        hover = true;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            manager.selectTile = gridPosition;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (manager.selectTile == gridPosition)
        {
            hover = true;      
        }

        if (!hover)
        { 
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, baseHeight, transform.position.z), dropSpeed);
        }
        else
        {
                transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, hoverDistance, transform.position.z), climbSpeed);
        }

        hover = false;
    }
}
