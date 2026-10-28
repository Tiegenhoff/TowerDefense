using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public partial class LevelC : MonoBehaviour
{
    [Header("Node Path Visuals")]
    public Material nodeLineMaterial;       // use a sprite material that works with your render pipeline
    public Color pathLineColor = Color.cyan;
    public Color nodeColor = Color.cyan;
    public Color bendColor = Color.yellow;
    public Color invalidPathColor = Color.red;
    public float nodeLineWidth = .05f;      // sizes are fractions of a tile
    public float nodeRadius = .18f;
    public float bendRadius = .12f;
    public float pathArrowSize = .2f;
    public int pathArrowSpacing = 3;

    private GameObject nodeVisualParent;
    private List<LineRenderer> nodeVisualLines = new List<LineRenderer>();
    private Material defaultNodeMaterial;
    private bool warnedNodeMaterial;
    private int usedNodeLines;
    private float nodeVisualScale;
    private Vector3 nodeVisualRight;
    private Vector3 nodeVisualUp;
    private Vector3 nodeVisualNormal;
    private int nodeVisualLayer;
    private int nodeVisualOrder;

    // draw the graph or a temporary drag preview
    private void DrawNodePaths(NodePathGraph graph, bool valid = true)
    {
        usedNodeLines = 0;

        if (graph == null || groundTilemap == null || !SetupNodeVisuals())
        {
            HideNodePaths();
            return;
        }

        nodeVisualParent.SetActive(true);
        Color lineColor = valid ? pathLineColor : invalidPathColor;

        foreach (var connection in graph.connections)
        {
            if (connection.cells == null || connection.cells.Count < 2)
                continue;

            Vector3[] positions = new Vector3[connection.cells.Count];
            for (int i = 0; i < connection.cells.Count; i++)
                positions[i] = groundTilemap.GetCellCenterWorld(connection.cells[i]);

            DrawNodeLine(positions, lineColor, false);

            // arrows point from the start node toward the end node
            int spacing = Mathf.Max(1, pathArrowSpacing);
            for (int i = 1; i < positions.Length; i += spacing)
            {
                Vector3 direction = (positions[i] - positions[i - 1]).normalized;
                if (direction.sqrMagnitude == 0)
                    continue;

                Vector3 middle = (positions[i] + positions[i - 1]) * .5f;
                Vector3 side = Vector3.Cross(nodeVisualNormal, direction).normalized;
                float arrowSize = Mathf.Max(.02f, pathArrowSize) * nodeVisualScale;
                Vector3 tip = middle + direction * arrowSize * .5f;

                Vector3[] arrow = new Vector3[3];
                arrow[0] = tip - direction * arrowSize + side * arrowSize * .6f;
                arrow[1] = tip;
                arrow[2] = tip - direction * arrowSize - side * arrowSize * .6f;
                DrawNodeLine(arrow, lineColor, false);
            }

            Vector3Int first = connection.cells[0];
            Vector3Int lastCell = connection.cells[connection.cells.Count - 1];
            if (first.x != lastCell.x && first.y != lastCell.y)
            {
                // this handle becomes a real node when moved or used for a branch
                DrawNodeCircle(graph.GetBend(connection), bendRadius,
                    valid ? bendColor : invalidPathColor);
            }
        }

        foreach (var node in graph.nodes)
            DrawNodeCircle(node.position, nodeRadius, valid ? nodeColor : invalidPathColor);

        // keep spare renderers for the next preview instead of destroying them
        for (int i = usedNodeLines; i < nodeVisualLines.Count; i++)
            nodeVisualLines[i].gameObject.SetActive(false);
    }

    private bool SetupNodeVisuals()
    {
        if (nodeLineMaterial == null && defaultNodeMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
            {
                if (!warnedNodeMaterial)
                {
                    Debug.LogWarning("Assign Node Line Material to show the path nodes and arrows.");
                    warnedNodeMaterial = true;
                }
                return false;
            }

            defaultNodeMaterial = new Material(shader);
        }

        if (nodeVisualParent == null)
        {
            nodeVisualParent = new GameObject("Node Path Visuals");
            nodeVisualParent.transform.SetParent(transform, false);
        }

        // use tilemap directions so rotated or scaled maps keep matching handles
        Vector3 origin = groundTilemap.GetCellCenterWorld(Vector3Int.zero);
        Vector3 right = groundTilemap.GetCellCenterWorld(new Vector3Int(1, 0, 0)) - origin;
        Vector3 up = groundTilemap.GetCellCenterWorld(new Vector3Int(0, 1, 0)) - origin;
        nodeVisualScale = Mathf.Max(.001f, Mathf.Min(right.magnitude, up.magnitude));
        nodeVisualRight = right.normalized;
        nodeVisualUp = up.normalized;
        nodeVisualNormal = Vector3.Cross(nodeVisualRight, nodeVisualUp).normalized;

        TilemapRenderer tileRenderer = groundTilemap.GetComponent<TilemapRenderer>();
        nodeVisualLayer = tileRenderer != null ? tileRenderer.sortingLayerID : 0;
        nodeVisualOrder = tileRenderer != null ? tileRenderer.sortingOrder + 10 : 10;
        return true;
    }

    private void DrawNodeCircle(Vector3Int cell, float radius, Color color)
    {
        Vector3 center = groundTilemap.GetCellCenterWorld(cell);
        Vector3[] positions = new Vector3[24];
        float size = Mathf.Max(.02f, radius) * nodeVisualScale;

        for (int i = 0; i < positions.Length; i++)
        {
            float angle = i * Mathf.PI * 2 / positions.Length;
            positions[i] = center + nodeVisualRight * Mathf.Cos(angle) * size
                + nodeVisualUp * Mathf.Sin(angle) * size;
        }

        DrawNodeLine(positions, color, true);
    }

    private void DrawNodeLine(Vector3[] positions, Color color, bool loop)
    {
        LineRenderer line;
        if (usedNodeLines < nodeVisualLines.Count)
        {
            line = nodeVisualLines[usedNodeLines];
        }
        else
        {
            GameObject lineObject = new GameObject("Path Line");
            lineObject.transform.SetParent(nodeVisualParent.transform, false);
            line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.numCapVertices = 2;
            line.numCornerVertices = 2;
            nodeVisualLines.Add(line);
        }

        usedNodeLines++;
        line.gameObject.SetActive(true);
        line.sharedMaterial = nodeLineMaterial != null ? nodeLineMaterial : defaultNodeMaterial;
        line.startColor = color;
        line.endColor = color;
        line.startWidth = Mathf.Max(.005f, nodeLineWidth) * nodeVisualScale;
        line.endWidth = line.startWidth;
        line.sortingLayerID = nodeVisualLayer;
        line.sortingOrder = nodeVisualOrder;
        line.loop = loop;
        line.positionCount = positions.Length;
        line.SetPositions(positions);
    }

    private void HideNodePaths()
    {
        if (nodeVisualParent != null)
            nodeVisualParent.SetActive(false);
    }

    // called by LevelC.OnDestroy; never destroy the material assigned in the Inspector
    private void CleanNodePathVisuals()
    {
        if (nodeVisualParent != null)
            Destroy(nodeVisualParent);
        if (defaultNodeMaterial != null)
            Destroy(defaultNodeMaterial);

        nodeVisualLines.Clear();
        nodeVisualParent = null;
        defaultNodeMaterial = null;
    }
}
