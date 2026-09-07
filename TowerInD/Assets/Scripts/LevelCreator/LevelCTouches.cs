using NUnit.Framework.Constraints;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class LevelC
{
    public bool selection;
    public int tileSelection;
    [System.NonSerialized] public TileC currentTile;
    
    public Camera mainCamera;

    private Coroutine runningClick;

    // Place Tile Routine
    public IEnumerator ClickTile()
    {
        Debug.Log($"Starting Clicks");
        while (selection)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Debug.Log($"Click");
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Vector3 mousePosWorld = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));
        
                mousePosWorld.z = 0;

                Vector3Int gridPosition = groundTilemap.WorldToCell(mousePosWorld);

                Debug.Log($"Grid position: {gridPosition}");

                /*if (groundMap[gridPosition.x, gridPosition.y].type == (int)TileType.PATH)
                    continue;*/

                // Set the tile to the current tiletype and save to the 2D array
                groundTilemap.SetTile(new Vector3Int(gridPosition.x, gridPosition.y, 0), tiles[currentTile.type]);
                groundMap[gridPosition.x, gridPosition.y] = currentTile;

            }

            yield return null;
        }
    }

    // Select the tile to place.
    public void SelectTile(int i)
    {
        if (i == tileSelection)
        {
            selection = false;
            if (runningClick != null)
                StopCoroutine(runningClick);
            tileSelection = -1;
            return;
        }

        // set the tile properties
        TileC temptile = new TileC();
        switch (i)
        {
            case (int)TileType.GRASS:
                temptile.placeable = true;
                break;
            case (int)TileType.PATH:
                temptile.placeable = false;
                break;
            case (int)TileType.WATER:
                temptile.placeable = false;
                break;
            default:

                break;
        }
        temptile.type = i;

        if (runningClick != null)
            StopCoroutine(runningClick);
        
        currentTile = temptile;
        selection = true;
        runningClick = StartCoroutine(ClickTile());
    }

}
