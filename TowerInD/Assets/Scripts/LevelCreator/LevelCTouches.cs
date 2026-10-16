using NUnit.Framework.Constraints;
using System.Collections;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;
using System.Collections.Generic;

public partial class LevelC
{
    public bool selection;
    public int tileSelection;
    [System.NonSerialized] public TileC currentTile;

    [System.NonSerialized] private TileC previousTile;
    private Vector3Int previousPath;

    public GameObject topBar;
    public GameObject endPathButtonPrefab;
    private GameObject currentEndPathButton;
    public GameObject sideBar;

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

                if (gridPosition.x >= groundMap.GetLength(0) || gridPosition.y >= groundMap.GetLength(1) || gridPosition.x < 0 || gridPosition.y < 0)
                {
                    Debug.Log($"[!!!] Outside of 2D Array, Skipped");
                    yield return null;
                    continue;
                }

                /*if (groundMap[gridPosition.x, gridPosition.y].type == (int)TileType.PATH)
                    continue;*/

                if (currentTile.type == (int)TileType.PATH)
                {
                    // Check if the patth placement is proper
                    if (start == null)
                    {
                        if (gridPosition.x != 0)
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

                    /*// Set the tile to the current tiletype
                    groundTilemap.SetTile(gridPosition, tiles[currentTile.type]);

                    // Record world position for PATH
                    currentTile.position = groundTilemap.GetCellCenterWorld(gridPosition);

                    // Save to the 2D array
                    groundMap[gridPosition.x, gridPosition.y] = currentTile;

                    if (start != null)
                        groundMap[previousPath.x, previousPath.y].next = currentTile;
                    else
                        start = currentTile;
                    previousPath = gridPosition;*/
                    
                    //!

                    // Set the visual tile
                    groundTilemap.SetTile(gridPosition, tiles[currentTile.type]);

                    // Create a separate TileC object for this placed tile
                    TileC placedTile = new TileC();

                    placedTile.type = currentTile.type;
                    placedTile.placeable = currentTile.placeable;
                    placedTile.position = groundTilemap.GetCellCenterWorld(gridPosition);

                    // Save the new tile in the array
                    groundMap[gridPosition.x, gridPosition.y] = placedTile;

                    // Connect the previous path tile to this one
                    if (previousTile != null)
                    {
                        previousTile.next = placedTile;
                    }
                    else
                    {
                        start = placedTile;
                    }

                    // This tile becomes the previous path tile
                    previousTile = placedTile;
                    previousPath = gridPosition;

                    //!

                    // Allow End
                    if (gridPosition.x == size - 1)
                    {
                        // Show end button
                        if (currentEndPathButton == null)
                        {
                            currentEndPathButton = Instantiate(endPathButtonPrefab);
                            currentEndPathButton.transform.SetParent(topBar.transform, false);
                            currentEndPathButton.GetComponent<Button>().onClick.AddListener(EndPlacement);
                        }
                    }
                    else
                    {
                        if (currentEndPathButton != null)
                            Destroy(currentEndPathButton);
                    }
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

    public void EndPlacement()
    {
        sideBar.SetActive(false);
        if (currentEndPathButton != null)
            Destroy(currentEndPathButton);
        selection = false;

        last = previousTile;

        // Generate the Routes
        GenerateRoutes();

        // testing purposes
        //StartCoroutine(TestDrive());
        testObj = Instantiate(testPrefab);
    }

    public void GenerateRoutes()
    {
        TileC currentAim = start;
        bool run = true;

        Routes route = new Routes();

        int currentCount = 0;
        List<bool> splits = new List<bool>();
        
        while (run)
        {
            route.path.Add(currentAim.position);

            // if currentAim bool split then repeat++;
            if (currentAim.split)
            {
                if (currentCount + 1 == splits.Count)
                {
                    splits[currentCount] = true;
                    currentCount++;
                }
                else if (splits.Count == currentCount)
                {
                    splits.Add(false);
                    currentCount++;
                }
            }

            // pick a path
            if (currentAim.split && (splits[currentCount - 1] == true))
                currentAim = currentAim.next2[1];
            else
                currentAim = currentAim.next2[0];

            // end? check if its any of the end points
            for (int i = 0; i < last2.Count; i++)
            {
                if (currentAim == last2[i])
                {
                    route.path.Add(currentAim.position);
                    
                    // clear out any dead ends
                    for (int x = splits.Count - 1; x >= 0; x--)
                    {
                        if (splits[x] == false)
                            break;
                        else if (splits[x] == true)
                        {
                            splits.RemoveAt(x);
                        }
                    }

                    if (splits.Count == 0)
                        run = false;

                    currentCount = 0;
                    currentAim = start;
                    tracks.Add(route);
                    route = new Routes();
                }
            }
        }

    }


    
    public GameObject testPrefab;
    private GameObject testObj;

    /*
    public IEnumerator TestDrive()
    {
        bool moving = true;

        TileC currentAim = start;
        testObj = Instantiate(testPrefab);

        testObj.transform.position = currentAim.position + new Vector3(0, 0, -1);
        currentAim = currentAim.next;

        while (moving)
        {
            // move it
            Debug.Log($"One Round Of Moving");
            yield return LerpPosition(testObj.transform, testObj.transform.position, currentAim.position + new Vector3(0, 0, -1), .25f);
            
            if (currentAim == last)
                moving = false;
            else
                currentAim = currentAim.next;

            yield return null;
        }
    }

    // temporary lerp
    private IEnumerator LerpPosition(
    Transform target,
    Vector3 startPosition,
    Vector3 endPosition,
    float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            target.position = Vector3.Lerp(
                startPosition,
                endPosition,
                t
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        target.position = endPosition;
    } */
}
