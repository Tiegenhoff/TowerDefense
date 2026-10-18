using NUnit.Framework.Constraints;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.UIElements;

public partial class LevelC
{
    public bool selection;
    public int tileSelection;
    [System.NonSerialized] public TileC currentTile;

    [System.NonSerialized] private TileC previousTile;
    private Vector3Int previousPath;

    // split path variants
    [System.NonSerialized] private List<TileC> previousTile2;
    private List<Vector3Int> previousPath2;

    // ui
    public GameObject topBar;
    public GameObject endPathButtonPrefab;
    private GameObject currentEndPathButton;
    public GameObject sideBar;

    public Camera mainCamera;

    private Coroutine runningClick;

    // testing
    [SerializeField] public List<Waves> eWavTest;
    public WaveCreation wc;

    // Place Tile Routine
    public IEnumerator ClickTile()
    {
        Debug.Log($"Starting Clicks");
        previousPath2 = new List<Vector3Int>();
        previousTile2 = new List<TileC>();
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

                    Vector3Int gWiz = IsPathPlacementProper(gridPosition);

                    if (start != null && (gWiz == new Vector3Int(999, 999)) )
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

                    // Set the visual tile
                    groundTilemap.SetTile(gridPosition, tiles[currentTile.type]);

                    // Create a separate TileC object for this placed tile
                    TileC placedTile = new TileC();

                    placedTile.type = currentTile.type;
                    placedTile.placeable = currentTile.placeable;
                    placedTile.position = groundTilemap.GetCellCenterWorld(gridPosition);
                    placedTile.next2 = new List<TileC>();

                    // Save the new tile in the array
                    groundMap[gridPosition.x, gridPosition.y] = placedTile;

                    // Connect the previous path tile to this one
                    if (previousTile != null)
                    {
                        groundMap[gWiz.x, gWiz.y].next2.Add(placedTile);
                        
                        previousPath = gWiz;
                        previousTile = groundMap[gWiz.x, gWiz.y];
                        //previousTile.next = placedTile;
                    }
                    else
                    {
                        start = placedTile;
                        //Debug.Log($"S check: {start.position}");
                    }

                    // remove tile from list before replacing and path
                    foreach (var tileC in previousTile2.ToList())
                    {
                        if (tileC == previousTile)
                        {
                            previousTile2.Remove(tileC);
                        }
                    }
                    foreach (var ppVec3Int in previousPath2.ToList())
                    {
                        if (ppVec3Int == previousPath)
                        {
                            previousPath2.Remove(ppVec3Int);
                        }
                    }

                    // This tile becomes the previous path tile
                    previousTile = placedTile;
                    previousTile2.Add(placedTile);
                    previousPath = gridPosition;
                    previousPath2.Add(gridPosition);


                    //!

                    // Allow End (only when the actual last tile that was placed was in the endzone
                    /*if (gridPosition.x == size - 1)
                    {
                        // Show end button
                        if (currentEndPathButton == null)
                        {
                            currentEndPathButton = Instantiate(endPathButtonPrefab);
                            currentEndPathButton.transform.SetParent(topBar.transform, false);
                            currentEndPathButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(EndConfirmation);
                        }
                    }
                    else
                    {
                        if (currentEndPathButton != null)
                            Destroy(currentEndPathButton);
                    }*/
                    int endingTime = 0;
                    foreach (var ppVect3Int in previousPath2)
                    {
                        if (ppVect3Int.x == size - 1)
                            endingTime += 0;
                        else
                            endingTime++;
                    }

                    if (endingTime == 0)
                    {
                        // Show end button
                        if (currentEndPathButton == null)
                        {
                            currentEndPathButton = Instantiate(endPathButtonPrefab);
                            currentEndPathButton.transform.SetParent(topBar.transform, false);
                            currentEndPathButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(EndConfirmation);
                        }
                    }
                    else
                    {
                        if (currentEndPathButton != null)
                        {
                            Destroy(currentEndPathButton);
                        }
                    }
                }
                else if (groundMap[gridPosition.x, gridPosition.y].type != (int)TileType.PATH)
                {
                    // Set the tile to the current tiletype
                    groundTilemap.SetTile(gridPosition, tiles[currentTile.type]);
                    
                    // Save to the 2D array
                    groundMap[gridPosition.x, gridPosition.y] = currentTile;
                }

                foreach(var t in previousPath2)
                    Debug.Log($"pp: {t.x} {t.y}");
            }
            if (Mouse.current.rightButton.wasPressedThisFrame) // Specifically for turning it into a split
            {
                Debug.Log($"Right Click!");
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Vector3 mousePosWorld = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));

                mousePosWorld.z = 0;

                Vector3Int gridPosition = groundTilemap.WorldToCell(mousePosWorld);

                // Debug.Log($"Grid position: {gridPosition}");

                if (gridPosition.x >= groundMap.GetLength(0) || gridPosition.y >= groundMap.GetLength(1) || gridPosition.x < 0 || gridPosition.y < 0)
                {
                    Debug.Log($"[!!!] Outside of 2D Array, Skipped");
                    yield return null;
                    continue;
                }

                /*if (groundMap[gridPosition.x, gridPosition.y].type == (int)TileType.PATH)
                    continue;*/

                if (currentTile.type == (int)TileType.PATH && groundMap[gridPosition.x, gridPosition.y].type == (int)TileType.PATH && groundMap[gridPosition.x, gridPosition.y].split != true)
                {
                    groundMap[gridPosition.x, gridPosition.y].split = true;
                    previousTile2.Add(groundMap[gridPosition.x, gridPosition.y]);
                    previousPath2.Add(gridPosition);

                    // setup previous tile
                    previousPath = gridPosition;
                    previousTile = groundMap[gridPosition.x, gridPosition.y];

                    // make visually different
                    groundTilemap.RemoveTileFlags(gridPosition, TileFlags.LockColor);
                    groundTilemap.SetColor(gridPosition, Color.yellow);
                }
                foreach (var t in previousPath2)
                    Debug.Log($"pp: {t.x} {t.y}");
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

    public Vector3Int IsPathPlacementProper(Vector3Int gPos)
    {
        if (groundMap[gPos.x, gPos.y].type == (int)TileType.PATH)
            return new Vector3Int(999, 999);

        // make sure an adjacent tile is a path
        for (int x = -1; x < 2; x++)
        {
            for (int y = -1;  y < 2; y++)
            {
                if (Math.Abs(x) == Math.Abs(y))
                    continue;

                foreach (var pPvect3Int in previousPath2)
                {
                    Vector3Int gWiz = new Vector3Int(gPos.x + x, gPos.y + y);
                    if (gWiz == pPvect3Int)
                    {
                        return gWiz;
                    }
                }
                /*if (new Vector3Int(gPos.x + x, gPos.y + y) == previousPath)
                    return true;*/
            }
        }



        return new Vector3Int(999, 999);
    }


    // confirms route placement
    public void EndConfirmation()
    {
        //last2.Add(previousTile);

        // generate last2 positions and likewise generate the Routes at the same time;
        GenerateRoutes();

        // remove the tile from being placed
        /*foreach (var tileC in previousTile2.ToList())
        {
            if (tileC == previousTile)
            {
                previousTile2.Remove(tileC);
            }
        }
        foreach (var ppVect3Int in previousPath2.ToList())
        {
            if (ppVect3Int == previousPath)
            {
                previousPath2.Remove(ppVect3Int);
            }
        }

        // if more tiles need to reach the end
        if (previousTile2.Count > 0)
            return;*/

        // ui
        sideBar.SetActive(false);
        if (currentEndPathButton != null)
            Destroy(currentEndPathButton);

        selection = false;

        // Generate the Routes
        //GenerateRoutes();

        // testing purposes
        //WaveCreation wc = new WaveCreation();
        wc.EnemySpawn(eWavTest, 1f, tracks, 5f);

    }

    public void GenerateRoutes()
    {
        TileC currentAim = start;
        //Debug.Log($"startinin checkerr: {start.next2[0].position}");
        bool run = true;

        Routes route = new Routes();
        route.path = new List<Vector3>();

        int currentCount = 0;
        List<bool> splits = new List<bool>();
        
        while (run)
        {
            Debug.Log($"HelloRun");
            route.path.Add(currentAim.position);

            // if currentAim bool split then repeat++;
            if (currentAim.split)
            {
                if (currentCount + 1 == splits.Count)
                {
                    splits[currentCount] = true;
                }
                else if (splits.Count == currentCount)
                {
                    splits.Add(false);
                }

                currentCount++;
            }

            //Debug.Log($"{currentAim.next2[0].position}")
            // pick a path
            if (currentAim.split && (splits[currentCount - 1] == true))
                currentAim = currentAim.next2[1];
            else if (currentAim.next2 == null || currentAim.next2.Count == 0)
                Debug.Log($"Ending Achieved (ignore this)");
            else
                currentAim = currentAim.next2[0];

            // end? check if its any of the end points
            //for (int i = 0; i < last2.Count; i++)
            if (true) // temporary
            {
                //if (currentAim == last2[i])
                if (currentAim.next2 == null || currentAim.next2.Count == 0)
                {
                    if (last2 == null)
                        last2 = new List<TileC>();
                    if (tracks == null)
                        tracks = new List<Routes>();

                    last2.Add(currentAim);

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
                    route.path = new List<Vector3>();
                }
            }
        }
        Debug.Log($"Route Generation Completed");

    }

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
