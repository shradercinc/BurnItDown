using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveEntity : Entity
{
    public virtual bool CanInteract()
    {
        return true;
    }

    public IEnumerator ObjectiveComplete()
    {
        yield return null;
    }
}
