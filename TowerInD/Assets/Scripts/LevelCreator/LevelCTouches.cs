using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public partial class LevelC : MonoBehaviour
{
    public bool selection;
    public int tileSelection = -1;
    [System.NonSerialized] public TileC currentTile;

    [Header("UI")]
    public GameObject topBar;
    public GameObject endPathButtonPrefab;
    private GameObject currentEndPathButton;
    public GameObject sideBar;
    public Camera mainCamera;

    private Coroutine runningClick;

    // normal terrain painting still uses individual tiles
    public IEnumerator ClickTile()
    {
        // ignore the click that selected the button
        yield return null;
        while (selection)
        {
            Vector3Int gridPosition;
            Vector2 position;
            if (mapReady && PointerPressed(out position) &&
                !PointerOverUI(position) && TryGetCell(position, out gridPosition))
            {
                if (groundMap[gridPosition.x, gridPosition.y].type != (int)TileType.PATH)
                {
                    TileC placedTile = new TileC();
                    placedTile.type = currentTile.type;
                    placedTile.placeable = currentTile.placeable;
                    placedTile.position = groundTilemap.GetCellCenterWorld(gridPosition);
                    placedTile.next2 = new List<TileC>();
                    groundMap[gridPosition.x, gridPosition.y] = placedTile;
                    groundTilemap.SetTile(gridPosition, tiles[placedTile.type]);
                }
            }
            yield return null;
        }
    }

    public void SelectTile(int i)
    {
        // old path buttons can use the new tool too
        if (i == (int)TileType.PATH)
        {
            SelectNodePath();
            return;
        }
        bool deselect = selection && i == tileSelection;
        StopPlacement();
        if (deselect || !mapReady || i < 0 || i >= tiles.Count)
            return;

        currentTile = new TileC();
        currentTile.type = i;
        currentTile.placeable = i == (int)TileType.GRASS;
        tileSelection = i;
        selection = true;
        runningClick = StartCoroutine(ClickTile());
    }

    private void StopPlacement()
    {
        selection = false;
        nodeSelection = false;
        tileSelection = -1;
        if (runningClick != null)
            StopCoroutine(runningClick);
        runningClick = null;
        CancelNodeDrag();
        HideNodePaths();
    }

    // Read a real touch on a phone and a mouse click in the editor.
    private bool PointerPressed(out Vector2 position)
    {
        position = Vector2.zero;
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.wasPressedThisFrame)
                {
                    position = touch.position.ReadValue();
                    return true;
                }
            }
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();
            return true;
        }
        return false;
    }

    private bool PointerOverUI(Vector2 position)
    {
        if (EventSystem.current == null)
            return false;
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = position;
        List<RaycastResult> hits = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, hits);
        foreach (RaycastResult hit in hits)
            if (hit.module is UnityEngine.UI.GraphicRaycaster)
                return true;
        return false;
    }

    private bool TryGetCell(Vector2 screenPosition, out Vector3Int cell)
    {
        cell = Vector3Int.zero;
        if (!mapReady || groundTilemap == null)
            return false;
        Camera camera = mainCamera != null ? mainCamera : Camera.main;
        if (camera == null)
            return false;
        Ray ray = camera.ScreenPointToRay(screenPosition);
        Plane plane = new Plane(groundTilemap.transform.forward, groundTilemap.GetCellCenterWorld(Vector3Int.zero));
        float distance;
        if (!plane.Raycast(ray, out distance))
            return false;
        cell = groundTilemap.WorldToCell(ray.GetPoint(distance));
        cell.z = 0;
        return CellInsideMap(cell);
    }

    public void EndConfirmation()
    {
        if (!GenerateNodeRoutes())
            return;
        StopPlacement();
        sideBar.SetActive(false);
        if (currentEndPathButton != null)
        {
            Destroy(currentEndPathButton);
            currentEndPathButton = null;
        }
        SetupWaveControl();
    }

    // keep the original public method for other scripts
    public void GenerateRoutes()
    {
        GenerateNodeRoutes();
    }
}
