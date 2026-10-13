using NUnit.Framework.Constraints;
using System.Collections;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class LevelC
{
    public bool selection;
    public int tileSelection;
    [System.NonSerialized] public TileC currentTile;

    [System.NonSerialized] private TileC previousTile;
    private Vector3Int previousPath;

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
                if (currentTile.type == (int)TileType.PATH)
                {
                    // Check if the patth placement is proper
                    if (start == null)
                    {
                        if (gridPosition.x != 0 && gridPosition.y != 0)
                        {
                            Debug.Log($"Invalid Starting Path Placement!");
                            yield return null;
                            continue;
                        }
                    }

                    if (start != null && !IsPathPlacementProper(gridPosition))
                    {
                        Debug.Log($"Invalid Path Placement!");
                        yield return null;
                        continue;
                    }

                    // Set the tile to the current tiletype
                    groundTilemap.SetTile(gridPosition, tiles[currentTile.type]);

                    // Record world position for PATH
                    currentTile.position = groundTilemap.GetCellCenterWorld(gridPosition);

                    // Save to the 2D array
                    groundMap[gridPosition.x, gridPosition.y] = currentTile;

                    if (start != null)
                        groundMap[previousPath.x, previousPath.y].next = currentTile;
                    else
                        start = currentTile;
                    previousPath = gridPosition;
                }
                else
                {
                    // Set the tile to the current tiletype
                    groundTilemap.SetTile(gridPosition, tiles[currentTile.type]);
                    
                    // Save to the 2D array
                    groundMap[gridPosition.x, gridPosition.y] = currentTile;
                }

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

    public bool IsPathPlacementProper(Vector3Int gPos)
    {
        if (groundMap[gPos.x, gPos.y].type == (int)TileType.PATH)
            return false;

        // make sure an adjacent tile is a path
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1;  y < 2; y++)
            {
                if (Math.Abs(x) == Math.Abs(y))
                    continue;

                if (new Vector3Int(gPos.x + x, gPos.y + y) == previousPath)
                    return true;
            }
        }



        return false;
    }
}
