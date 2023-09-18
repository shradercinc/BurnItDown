using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    public Vector2Int CurrentGrid;
    [SerializeField] public int movementSpeed = 3;
    public bool patrol;
    public GridManager manager;

    public void enemyEndTurn()
    {
        
    }


}
