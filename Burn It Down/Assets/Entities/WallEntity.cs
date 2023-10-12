using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallEntity : Entity
{
    [Foldout("Wall Entity",true)]
        [Tooltip("Health a wall has")][ReadOnly] public int health;

    public override string HoverBoxText()
    {
        return "Current Health: " + health;
    }
}
