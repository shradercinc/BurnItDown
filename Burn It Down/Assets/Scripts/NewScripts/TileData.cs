using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class TileData : MonoBehaviour
{
    [Tooltip("All adjacent tiles")] public List<TileData> adjacentTiles;
    [Tooltip("Position in the grid")] public Vector2Int gridPosition;
    [Tooltip("How high to rise when selected")][SerializeField] float hoverDistance;
    [Tooltip("Rising speed when selected")][SerializeField] float climbSpeed = 0.9f;
    [Tooltip("How low to fall when unselected")] float baseHeight = 0;
    [Tooltip("Falling speed when unselected")][SerializeField] float dropSpeed = 0f;
    [Tooltip("Is hovering or not")] bool hover = false;
    [Tooltip("The entity on this tile")] public Entity myEntity;
    [Tooltip("Default texture")][SerializeField] Material defaultTexture;
    [Tooltip("Texture when under surveillance")][SerializeField] Material surveillanceTexture;
    [Tooltip("The entity on this tile")] public bool underSurveillance = false;
    [Tooltip("Renders the material")] MeshRenderer currentMaterial;
    private void Awake()
    {
        currentMaterial = GetComponent<MeshRenderer>();
    }
}