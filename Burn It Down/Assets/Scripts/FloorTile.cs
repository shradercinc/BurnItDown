using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FloorTile : MonoBehaviour
{
   
    public Vector2Int gridPosition;
    [SerializeField] float hoverDistance;
    [SerializeField] float climbSpeed = 0.9f;
    [SerializeField] float dropSpeed;
    private bool hover = false;
    private float baseHeight = 0;
    public GridManager manager;
    public ObjectManager AttachedObject;
    // Start is called before the first frame update

    private void OnMouseOver()
    {
        hover = true;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //selects this tile if it's not selected (unselects all tiles otherwise)
            if (manager.selectTile != gridPosition)
            {
                //checks if an object was selected
                if (manager.selectObject != null)
                {
                    //checks if selected object was a player
                    if (manager.selectObject.gameObject.tag == "Player")
                    {
                        //moves the player depending on their assigned movement speed, asigning them to this tile
                        if (MathF.Abs(gridPosition.x - manager.selectTile.x) + MathF.Abs(gridPosition.y - manager.selectTile.y) <= manager.selectObject.movementSpeed)
                        {
                            AttachedObject = manager.selectObject;
                            manager._Grid[manager.selectTile.x, manager.selectTile.y].AttachedObject = null;
                            AttachedObject.transform.parent = transform;
                            AttachedObject.transform.position = new Vector3(gridPosition.x * manager.tileSize, transform.position.y + manager.tileSize, gridPosition.y * -manager.tileSize);
                        }
                    }
                }
                manager.selectTile = gridPosition;
            }
            else
            {
                manager.selectTile = new Vector2Int(0, 0);
            }

        }
    }


    // Update is called once per frame
    void Update()
    {
        //if this tile is selected, it holds hover and changes the selected object in the manager
        if (manager.selectTile == gridPosition)
        {
            hover = true;
            manager.selectObject = AttachedObject;
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
