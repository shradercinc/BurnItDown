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

    public void AffectWall(int effect)
    {
        health += effect;
        if (health <= 0)
        {
            NewManager.instance.listOfWalls.Remove(this);
            Destroy(this.gameObject);
        }
    }
}
