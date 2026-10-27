// Author: Brandon Birman
// Current Revision Date: 10/26/2026
// Purpose: Allows placement of a set number of turrets
// References: Most of the PlaceTurrets code is taken from LevelCTouches by Austin to track mouse position

using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;

public partial class LevelC : MonoBehaviour
{
    public int numberTurrets;
    public GameObject turretPrefab;
    private Vector3 turretSpawnPos;
    private bool placingTurrets = true;

    public void StartPlaceTurrets()
    {
        Debug.Log($"Starting PlaceTurrets");
        StartCoroutine(PlaceTurrets());
    }

    public IEnumerator PlaceTurrets()
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

                if (gridPosition.x >= groundMap.GetLength(0) || gridPosition.y >= groundMap.GetLength(1) || gridPosition.x < 0 || gridPosition.y < 0)
                {
                    Debug.Log($"[!!!] Outside of 2D Array, Skipped");
                    yield return null;
                    continue;
                }
                
                // If mouse position is legal, place turret on mouse pos
                turretSpawnPos = mousePosWorld;
                Instantiate(turretPrefab, turretSpawnPos, Quaternion.identity);
                numberTurrets--;
                Debug.Log($"Turret placed");
            }

            // Stop while loop if number of turrets reaches 0
            if (numberTurrets <= 0)
            {
                placingTurrets = false;
            }

            yield return null;
        }
    }
}