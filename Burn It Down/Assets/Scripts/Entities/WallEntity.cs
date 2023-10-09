using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallEntity : Entity
{
    [Foldout("WallEntity",true)]
        [Tooltip("Health a wall has")][SerializeField]public int health = 3;

    public override string HoverBoxName()
    {
        return "Wall";
    }

    public override string HoverBoxText()
    {
        return "Current Health: " + health;
    }
}
