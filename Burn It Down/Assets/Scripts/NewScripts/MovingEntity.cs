using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class MovingEntity : Entity
{
    [Foldout("Moving Entity", true)]
        [Tooltip("Remaining moves")] public int movementLeft;
        [Tooltip("How many tiles this moves per turn")]public int movesPerTurn;
    
    public virtual IEnumerator EndOfTurn()
    {
        yield return null;
    }
}
