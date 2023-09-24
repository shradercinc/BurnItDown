using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;

public class TileData : MonoBehaviour
{
    [Tooltip("All adjacent tiles")] [ReadOnly] public List<TileData> adjacentTiles;
    [Tooltip("Position in the grid")] [ReadOnly] public Vector2Int gridPosition;
    [Tooltip("The entity on this tile")] [ReadOnly] public Entity myEntity;
    [Tooltip("Default texture")][SerializeField] Material defaultTexture;
    [Tooltip("Texture when under surveillance")][SerializeField] Material surveillanceTexture;
    [Tooltip("Renders the material")] MeshRenderer currentMaterial;
    [Tooltip("This can be clicked on")] [ReadOnly] public bool clickable = false;
    [Tooltip("The glowing border when this can be clicked")] SpriteRenderer border;

    private void Awake()
    {
        currentMaterial = GetComponent<MeshRenderer>();
        border = this.transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    void FixedUpdate()
    {
        if (border == null)
        {
            Debug.Log("border is null");
        }
        if (border != null && clickable)
        {
            border.color = new Color(1, 1, 1, ChoiceManager.instance.opacity);
        }
        else if (border != null && !clickable)
        {
            border.color = new Color(1, 1, 1, 0);
        }
    }

    private void OnMouseOver()
    {
        if (clickable && Input.GetKeyDown(KeyCode.Mouse0))
        {
            ChoiceManager.instance.ReceiveChoice(this);
        }
    }

    public void SurveillanceState(bool underSurveillance)
    {
        currentMaterial.material = (underSurveillance) ? surveillanceTexture : defaultTexture;
    }
}