using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class PlayerEntity : MovingEntity
{
    [Foldout("Player Entity", true)]
        [Tooltip("turns where you can't be caught")] [ReadOnly] public int hidden = 0;
        //[Tooltip("normal player appearance")] [SerializeField] Material DefaultPlayerMaterial;
        //[Tooltip("appearance when hidden")] [SerializeField] Material HiddenPlayerMaterial;
        [Tooltip("adjacent objective")][ReadOnly] public ObjectiveEntity adjacentObjective;

    public override string HoverBoxText()
    {
        return $"Moves left: {movementLeft}";
    }

    public override void MoveTile(TileData newTile)
    {
        base.MoveTile(newTile);
        foreach (GuardEntity guard in NewManager.instance.listOfGuards)
        {
            guard.CheckForPlayer();
        }
        NewManager.instance.objectiveButton.gameObject.SetActive(CheckForObjectives());
    }

    public bool CheckForObjectives()
    {
        for (int i = 0; i < this.currentTile.adjacentTiles.Count; i++)
        {
            TileData nextTile = this.currentTile.adjacentTiles[i];
            if (nextTile.myEntity != null && nextTile.myEntity.CompareTag("Objective"))
            {
                this.adjacentObjective = nextTile.myEntity.GetComponent<ObjectiveEntity>();
                return adjacentObjective.CanInteract();
            }
        }

        return false;
    }

    public override IEnumerator EndOfTurn()
    {
        yield return null;
        if (hidden > 0)
            hidden--;

        //meshRenderer.material = (hidden > 0) ? HiddenPlayerMaterial : DefaultPlayerMaterial;
    }
}
