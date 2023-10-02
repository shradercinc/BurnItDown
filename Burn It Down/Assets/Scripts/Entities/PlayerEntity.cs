using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PlayerEntity : MovingEntity
{
    [Foldout("Player Entity", true)]
        [Tooltip("turns where you can't be caught")] [ReadOnly] public int hidden = 0;
        [Tooltip("normal player appearance")] [SerializeField] Material DefaultPlayerMaterial;
        [Tooltip("appearance when hidden")] [SerializeField] Material HiddenPlayerMaterial;

    public override IEnumerator EndOfTurn()
    {
        yield return null;
        if (hidden > 0)
            hidden--;

        meshRenderer.material = (hidden > 0) ? HiddenPlayerMaterial : DefaultPlayerMaterial;
    }
}
