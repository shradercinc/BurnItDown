using UnityEngine;
using MyBox;

[System.Serializable]
public class EntityAndPosition
{
    public enum EntityType {Player, Wall, Guard};
    [Tooltip("Entity that should be spawned")]public EntityType entity;
    [Tooltip("Their starting position in the grid")]public Vector2 startingPosition;
    [ConditionalField(nameof(entity), false, EntityType.Guard)] public Vector2Int direction;
}
