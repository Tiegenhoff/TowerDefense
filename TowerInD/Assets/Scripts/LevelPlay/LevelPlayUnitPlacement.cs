using NUnit.Framework.Interfaces;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public partial class LevelPlay : MonoBehaviour
{
    //public int numberTurrets;
    [Header("TempSetupTurret")]
    public GameObject turretPrefab;
    public GameObject slowTurretPrefab;
    public GameObject pulseTurretPrefab;
    private GameObject selectedTurretPrefab;

    // Assign these methods to turret buttons in the gameplay UI.
    public void SelectBasicTurret() { SelectTurret(turretPrefab); }
    public void SelectSlowTurret() { SelectTurret(slowTurretPrefab); }
    public void SelectPulseTurret() { SelectTurret(pulseTurretPrefab); }

    private void SelectTurret(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Assign this turret prefab in the LevelPlay Inspector.");
            return;
        }
        if (placingTurrets)
        {
            if (selectedTurretPrefab == prefab)
            {
                placingTurrets = false;
                if (placementRoutine != null) StopCoroutine(placementRoutine);
                placementRoutine = null;
                return;
            }
            selectedTurretPrefab = prefab;
            return;
        }
        selectedTurretPrefab = prefab;
        StartPlacement();
    }

    private Vector3 turretSpawnPos;
    private bool placingTurrets = false;
    private Coroutine placementRoutine;

    // for checking if a turret is already there
    private bool[,] unitPlacement;

    // Preserve the existing basic turret button.
    public void StartPlaceTurrets() { SelectBasicTurret(); }

    private void StartPlacement()
    {
        if (selectedTurretPrefab == null)
            selectedTurretPrefab = turretPrefab;
        if (selectedTurretPrefab == null || groundMap == null)
        {
            Debug.LogWarning("Create the map and assign a turret prefab before placing turrets.");
            return;
        }
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
            if (placementRoutine != null) StopCoroutine(placementRoutine);
            placementRoutine = null;
            Debug.Log($"Stop Placement");
            return;
        }
        else if (!placingTurrets)
        {
            placingTurrets = true;
            Debug.Log($"Starting PlaceTurrets");
            placementRoutine = StartCoroutine(PlaceTurrets());
        }
        
    }

    private IEnumerator PlaceTurrets()
    {
        // Ignore the UI button press that started placement.
        yield return null;
        while (placingTurrets)
        {
            Vector2 mousePos;
            if (GetPlacementPress(out mousePos) && !PlacementPointerOverUI(mousePos))
            {
                Debug.Log($"Click");
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
                Instantiate(selectedTurretPrefab, turretSpawnPos, Quaternion.identity);
                //numberTurrets--;
                Debug.Log($"Turret placed");

                // endplacement
                startedPlacement = true; // calls to start the waves
                placingTurrets = false;
            }

            yield return null;
        }
    }



    private bool GetPlacementPress(out Vector2 position)
    {
        position = Vector2.zero;
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
                if (touch.press.wasPressedThisFrame)
                {
                    position = touch.position.ReadValue();
                    return true;
                }
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            return true;
        }
        return false;
    }

    private bool PlacementPointerOverUI(Vector2 position)
    {
        if (EventSystem.current == null) return false;
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = position;
        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        foreach (RaycastResult hit in hits)
            if (hit.module is UnityEngine.UI.GraphicRaycaster) return true;
        return false;
    }

}
