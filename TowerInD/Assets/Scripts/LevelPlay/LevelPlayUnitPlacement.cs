using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class LevelPlay : MonoBehaviour
{
    //public int numberTurrets;
    [Header("TempSetupTurret")]
    public GameObject turretPrefab;

    private Vector3 turretSpawnPos;
    private bool placingTurrets = false;

    // for checking if a turret is already there
    private bool[,] unitPlacement;

    public void StartPlaceTurrets()
    {
        // check for fist time setup
        if (unitPlacement == null)
        {
            unitPlacement = new bool[groundMap.GetLength(0), groundMap.GetLength(1)];
        }

        // would probably check for necessary materials beforehand.

        if (placingTurrets)
        {
            // allow the user to cancel placement
            placingTurrets = false;
            Debug.Log($"Stop Placement");
            return;
        }
        else if (!placingTurrets)
        {
            placingTurrets = true;
            Debug.Log($"Starting PlaceTurrets");
            StartCoroutine(PlaceTurrets());
        }
        
    }

    private IEnumerator PlaceTurrets()
    {
        while (placingTurrets)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                Debug.Log($"Click");
                Vector2 mousePos = Mouse.current.position.ReadValue();
                Vector3 mousePosWorld = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 0));

                mousePosWorld.z = -1;

                Vector3Int gridPosition = groundTilemap.WorldToCell(mousePosWorld);

                Debug.Log($"Grid position: {gridPosition}");

                // position checking
                if (gridPosition.x >= groundMap.GetLength(0) || gridPosition.y >= groundMap.GetLength(1) || gridPosition.x < 0 || gridPosition.y < 0)
                {
                    Debug.Log($"[!!!] Outside of 2D Array, Skipped");
                    yield return null;
                    continue;
                }
                if (!groundMap[gridPosition.x, gridPosition.y].placeable || unitPlacement[gridPosition.x, gridPosition.y])
                {
                    Debug.Log($"Not Valid Placement");
                    yield return null;
                    continue;
                }

                // set spot taken
                unitPlacement[gridPosition.x, gridPosition.y] = true;

                // convert gridPosition backt to work for precise middle tile placement
                Vector3 worldPos = groundTilemap.CellToWorld(gridPosition);
                worldPos.x += .5f;
                worldPos.y += .5f;
                worldPos.z = -1; // so it appears above

                // If mouse position is legal, place turret on mouse pos
                turretSpawnPos = worldPos;
                Instantiate(turretPrefab, turretSpawnPos, Quaternion.identity);
                //numberTurrets--;
                Debug.Log($"Turret placed");

                // endplacement
                startedPlacement = true; // calls to start the waves
                placingTurrets = false;
            }

            yield return null;
        }
    }



}
