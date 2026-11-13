using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public partial class LevelC : MonoBehaviour
{
    [Header("Node Paths")]
    public bool horizontalFirst = true;
    public TMP_Text pathStatusText;       // optional on-screen feedback
    [TextArea] public string pathMessage;
    [System.NonSerialized] public NodePathGraph nodePaths = new NodePathGraph();

    [Tooltip("Hold this long on a node or path before dragging to move it.")]
    public float nodeMoveHoldSeconds = .45f;
    [Tooltip("Screen pixels a finger can drift while holding.")]
    public float nodeDragThreshold = 15f;
    private bool nodeSelection;
    private bool deleteNodeMode;
    private NodePathGraph dragBase;
    private NodePathGraph dragPreview;
    private int dragNode;
    private Vector3Int dragStart;
    private Vector3Int dragCell;
    private bool movingNode;
    private bool previewValid;
    private bool previewChanged;
    private string previewMessage;

    // terrain hidden by the committed paths
    private List<NodePathGround> pathGround = new List<NodePathGround>();

    // connect the new button to this method
    public void SelectNodePath()
    {
        bool deselect = nodeSelection && !deleteNodeMode;
        deleteNodeMode = false;
        StopPlacement();
        if (deselect || !mapReady)
            return;
        if (HasUntrackedPathTiles())
        {
            SetPathMessage("These tiles have no node graph. Load a node-based save or start a new map to edit paths.");
            return;
        }
        nodeSelection = true;
        tileSelection = (int)TileType.PATH;
        nodePaths.NormalizeNodes();
        DrawNodePaths(nodePaths);
        SetPathMessage("Drag to draw or branch. Hold, then drag to move. Corners are nodes.");
        runningClick = StartCoroutine(ClickNodePath());
    }

    // Connect a UI button's OnClick to this method. Press again to leave delete mode.
    public void ToggleDeleteNodeMode()
    {
        bool turnOff = nodeSelection && deleteNodeMode;
        StopPlacement();
        deleteNodeMode = !turnOff;
        if (turnOff || !mapReady)
            return;
        if (HasUntrackedPathTiles())
        {
            deleteNodeMode = false;
            SetPathMessage("These tiles have no node graph. Load a node-based save or start a new map to edit paths.");
            return;
        }
        nodeSelection = true;
        tileSelection = (int)TileType.PATH;
        DrawNodePaths(nodePaths);
        SetPathMessage("Delete mode: tap a node or path to delete it. Tap Delete again to exit.");
        runningClick = StartCoroutine(ClickNodePath());
    }

    private IEnumerator ClickNodePath()
    {
        yield return null; // ignore the UI button press
        bool tracking = false;
        bool touchInput = false;
        int touchId = -1;
        Vector2 startPosition = Vector2.zero;
        Vector3Int startCell = Vector3Int.zero;
        float startTime = 0f;
        bool gestureStarted = false;

        while (nodeSelection)
        {
            Vector2 position = Vector2.zero;
            bool pressed = false, held = false, released = false, cancelled = false;
            if (!tracking)
            {
                if (Touchscreen.current != null)
                {
                    foreach (var touch in Touchscreen.current.touches)
                    {
                        if (!touch.press.wasPressedThisFrame) continue;
                        position = touch.position.ReadValue();
                        touchId = touch.touchId.ReadValue();
                        touchInput = true;
                        pressed = held = true;
                        break;
                    }
                }
                if (!pressed && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    position = Mouse.current.position.ReadValue();
                    touchInput = false;
                    pressed = held = true;
                }
            }
            else if (touchInput)
            {
                bool found = false;
                if (Touchscreen.current != null)
                {
                    foreach (var touch in Touchscreen.current.touches)
                    {
                        if (touch.touchId.ReadValue() != touchId) continue;
                        found = true;
                        position = touch.position.ReadValue();
                        var phase = touch.phase.ReadValue();
                        cancelled = phase == UnityEngine.InputSystem.TouchPhase.Canceled;
                        released = touch.press.wasReleasedThisFrame || phase == UnityEngine.InputSystem.TouchPhase.Ended;
                        held = touch.press.isPressed;
                        break;
                    }
                }
                if (!found) cancelled = true;
            }
            else if (Mouse.current != null)
            {
                position = Mouse.current.position.ReadValue();
                held = Mouse.current.leftButton.isPressed;
                released = Mouse.current.leftButton.wasReleasedThisFrame;
                if (!held && !released) cancelled = true;
            }
            else cancelled = true;

            Vector3Int cell = Vector3Int.zero;
            bool valid = !PointerOverUI(position) && TryGetCell(position, out cell);
            if (!valid) cell = Vector3Int.zero;
            if (pressed && valid)
            {
                tracking = true;
                startPosition = position;
                startCell = cell;
                startTime = Time.unscaledTime;
                gestureStarted = false;
            }
            if (tracking && !deleteNodeMode && !gestureStarted && held)
            {
                bool existing = nodePaths.NodeAt(startCell) != null || ConnectionAt(nodePaths, startCell) != null;
                if (existing && Vector2.Distance(position, startPosition) < nodeDragThreshold &&
                    Time.unscaledTime - startTime >= nodeMoveHoldSeconds)
                {
                    gestureStarted = true;
                    BeginNodeDrag(startCell, false);
                    SetPathMessage("Drag to move this node, then release.");
                }
                else if (Vector2.Distance(position, startPosition) >= nodeDragThreshold)
                {
                    gestureStarted = true;
                    BeginNodeDrag(startCell, true);
                }
            }
            if (tracking && gestureStarted && !cancelled)
            {
                if (valid && (cell != dragCell || dragPreview == null || !previewValid))
                    PreviewNodeDrag(cell);
                else if (!valid && previewValid)
                {
                    previewValid = false;
                    previewMessage = "Release inside the map, away from UI.";
                    DrawNodePaths(dragPreview ?? dragBase, false);
                }
            }
            if (tracking && (released || cancelled))
            {
                if (!cancelled && valid)
                {
                    if (deleteNodeMode && Vector2.Distance(position, startPosition) < nodeDragThreshold)
                        DeleteNodePathAt(cell);
                    else if (gestureStarted && previewValid && previewChanged)
                        CommitNodeGraph(dragPreview);
                    else if (!gestureStarted && cell == startCell && nodePaths.NodeAt(cell) == null && Vector2.Distance(position, startPosition) < nodeDragThreshold)
                        FlipNodePathAt(cell);
                }
                if (gestureStarted && !previewValid && !cancelled)
                    SetPathMessage(previewMessage);
                CancelNodeDrag();
                tracking = false;
            }
            yield return null;
        }
    }

    private void BeginNodeDrag(Vector3Int cell, bool branch)
    {
        dragBase = nodePaths.Copy();
        bool existing = dragBase.NodeAt(cell) != null || ConnectionAt(dragBase, cell) != null;
        NodePathNode node = GetOrMakeNode(dragBase, cell);
        dragNode = node.id;
        dragStart = cell;
        dragCell = cell;
        movingNode = existing && !branch;
        dragPreview = null;
        previewChanged = false;
    }

    private void PreviewNodeDrag(Vector3Int cell)
    {
        dragCell = cell;
        dragPreview = dragBase.Copy();
        previewChanged = cell != dragStart;
        if (movingNode)
        {
            dragPreview.FindNode(dragNode).position = cell;
        }
        else if (previewChanged)
        {
            NodePathNode endNode = GetOrMakeNode(dragPreview, cell);
            dragPreview.AddConnection(dragNode, endNode.id, horizontalFirst);
        }
        dragPreview.RebuildCells();
        previewValid = dragPreview.Validate(sizeM, heightSize, false, out previewMessage);
        DrawNodePaths(dragPreview, previewValid);
        if (!previewValid)
            SetPathMessage(previewMessage);
        else if (previewChanged)
            SetPathMessage("Release to apply. Move outside the map to cancel.");
    }

    private NodePathNode GetOrMakeNode(NodePathGraph graph, Vector3Int cell)
    {
        NodePathNode node = graph.NodeAt(cell);
        if (node != null)
            return node;
        NodePathConnection connection = ConnectionAt(graph, cell);
        if (connection != null)
            return graph.SplitConnection(connection.id, cell);
        return graph.AddNode(cell);
    }

    private NodePathConnection ConnectionAt(NodePathGraph graph, Vector3Int cell)
    {
        foreach (NodePathConnection connection in graph.connections)
        {
            if (connection.cells.Contains(cell))
                return connection;
        }
        return null;
    }

    private void CancelNodeDrag()
    {
        dragBase = null;
        dragPreview = null;
        previewChanged = false;
        if (nodeSelection)
            DrawNodePaths(nodePaths);
    }

    private void DeleteNodePathAt(Vector3Int cell)
    {
        NodePathGraph candidate = nodePaths.Copy();
        NodePathNode node = candidate.NodeAt(cell);
        if (node != null)
        {
            candidate.connections.RemoveAll(c => c.from == node.id || c.to == node.id);
            candidate.nodes.Remove(node);
        }
        else
        {
            NodePathConnection connection = ConnectionAt(candidate, cell);
            if (connection == null)
                return;
            candidate.connections.Remove(connection);
        }
        CommitNodeGraph(candidate);
    }

    private void FlipNodePathAt(Vector3Int cell)
    {
        NodePathGraph candidate = nodePaths.Copy();
        NodePathConnection connection = ConnectionAt(candidate, cell);
        if (connection == null)
            return;
        connection.horizontalFirst = !connection.horizontalFirst;
        CommitNodeGraph(candidate);
    }

    private bool CommitNodeGraph(NodePathGraph candidate)
    {
        string message;
        candidate.NormalizeNodes();
        if (!candidate.Validate(sizeM, heightSize, false, out message))
        {
            SetPathMessage(message);
            return false;
        }
        nodePaths = candidate;
        RebuildNodeTiles();
        UpdateNodeEndButton();
        if (nodeSelection)
            DrawNodePaths(nodePaths);
        return true;
    }

    // restore the old underlay first, then paint the new footprint
    private void RebuildNodeTiles()
    {
        foreach (NodePathGround saved in pathGround)
        {
            TileC tile = NewGroundTile(saved.position, saved.type, saved.placeable);
            groundMap[saved.position.x, saved.position.y] = tile;
            groundTilemap.SetTile(saved.position, tiles[saved.type]);
        }
        pathGround.Clear();
        HashSet<Vector3Int> painted = new HashSet<Vector3Int>();
        foreach (NodePathConnection connection in nodePaths.connections)
        {
            foreach (Vector3Int cell in connection.cells)
                PaintNodeCell(cell, painted);
        }
        foreach (NodePathNode node in nodePaths.nodes)
            PaintNodeCell(node.position, painted);

        // keep the tile links useful for existing code; routes use graph edges
        foreach (NodePathConnection connection in nodePaths.connections)
        {
            for (int i = 0; i < connection.cells.Count - 1; i++)
            {
                Vector3Int a = connection.cells[i];
                Vector3Int b = connection.cells[i + 1];
                TileC from = groundMap[a.x, a.y];
                TileC to = groundMap[b.x, b.y];
                if (!from.next2.Contains(to))
                    from.next2.Add(to);
                from.next = from.next2[0];
                from.split = from.next2.Count > 1;
            }
        }
        tracks = new List<Routes>();
        last2 = new List<TileC>();
        start = null;
        last = null;
        foreach (NodePathNode node in nodePaths.nodes)
        {
            bool incoming = false;
            bool outgoing = false;
            foreach (NodePathConnection connection in nodePaths.connections)
            {
                if (connection.to == node.id) incoming = true;
                if (connection.from == node.id) outgoing = true;
            }
            TileC tile = groundMap[node.position.x, node.position.y];
            if (!incoming && start == null) start = tile;
            if (!outgoing) last2.Add(tile);
        }
        if (last2.Count > 0) last = last2[0];
    }

    private void PaintNodeCell(Vector3Int cell, HashSet<Vector3Int> painted)
    {
        if (!painted.Add(cell))
            return;
        TileC oldTile = groundMap[cell.x, cell.y];
        pathGround.Add(new NodePathGround
        {
            position = cell,
            type = oldTile.type,
            placeable = oldTile.placeable
        });
        groundMap[cell.x, cell.y] = NewGroundTile(cell, (int)TileType.PATH, false);
        groundTilemap.SetTile(cell, tiles[(int)TileType.PATH]);
        groundTilemap.RemoveTileFlags(cell, TileFlags.LockColor);
        groundTilemap.SetColor(cell, Color.white);
    }

    private TileC NewGroundTile(Vector3Int cell, int type, bool placeable)
    {
        TileC tile = new TileC();
        tile.type = type;
        tile.placeable = placeable;
        tile.position = groundTilemap.GetCellCenterWorld(cell);
        tile.next2 = new List<TileC>();
        return tile;
    }

    private bool CellInsideMap(Vector3Int cell)
    {
        return groundMap != null && cell.x >= 0 && cell.y >= 0 &&
            cell.x < groundMap.GetLength(0) && cell.y < groundMap.GetLength(1);
    }

    private bool HasUntrackedPathTiles()
    {
        HashSet<Vector3Int> covered = new HashSet<Vector3Int>();
        foreach (NodePathNode node in nodePaths.nodes)
            covered.Add(node.position);
        foreach (NodePathConnection connection in nodePaths.connections)
        {
            foreach (Vector3Int cell in connection.cells)
                covered.Add(cell);
        }
        for (int x = 0; x < groundMap.GetLength(0); x++)
        {
            for (int y = 0; y < groundMap.GetLength(1); y++)
            {
                if (groundMap[x, y].type == (int)TileType.PATH && !covered.Contains(new Vector3Int(x, y, 0)))
                    return true;
            }
        }
        return false;
    }

    private void UpdateNodeEndButton()
    {
        string message;
        bool complete = nodePaths.Validate(sizeM, heightSize, true, out message);
        if (complete && currentEndPathButton == null && topBar != null && endPathButtonPrefab != null)
        {
            currentEndPathButton = Instantiate(endPathButtonPrefab, topBar.transform, false);
            currentEndPathButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(EndConfirmation);
        }
        else if (!complete && currentEndPathButton != null)
        {
            Destroy(currentEndPathButton);
            currentEndPathButton = null;
        }
        SetPathMessage(complete ? "Paths ready. Confirm to set up waves, or keep editing." : message);
    }

    private bool GenerateNodeRoutes()
    {
        if (!mapReady)
            return false;
        tracks = new List<Routes>();
        string message;
        if (!nodePaths.Validate(sizeM, heightSize, true, out message))
        {
            SetPathMessage(message);
            return false;
        }
        List<List<Vector3Int>> cellRoutes;
        if (!nodePaths.BuildRoutes(out cellRoutes, out message))
        {
            SetPathMessage(message);
            return false;
        }
        foreach (List<Vector3Int> cells in cellRoutes)
        {
            Routes route = new Routes();
            route.path = new List<Vector3>();
            foreach (Vector3Int cell in cells)
                route.path.Add(groundTilemap.GetCellCenterWorld(cell));
            tracks.Add(route);
        }
        SetPathMessage("Generated " + tracks.Count + " routes.");
        return true;
    }

    private void SetPathMessage(string message)
    {
        pathMessage = message;
        if (pathStatusText != null)
            pathStatusText.text = message;
    }

    private void ResetNodePaths()
    {
        StopPlacement();
        nodePaths = new NodePathGraph();
        pathGround = new List<NodePathGround>();
        tracks = new List<Routes>();
        last2 = new List<TileC>();
        start = null;
        last = null;
        if (currentEndPathButton != null)
        {
            Destroy(currentEndPathButton);
            currentEndPathButton = null;
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            StopPlacement();
    }

    private void OnDisable()
    {
        StopPlacement();
    }

    private void OnDestroy()
    {
        CleanNodePathVisuals();
    }
}
