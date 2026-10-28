using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public partial class LevelC : MonoBehaviour
{
    // keep the old JSON field names so gameplay can still read LevelData
    public void SaveToJSON(string saveName)
    {
        if (!GenerateNodeRoutes())
        {
            Debug.LogWarning("Finish the paths before saving. " + pathMessage);
            return;
        }

        MapSave newMSAVE = new MapSave();
        newMSAVE.mapColumnSaves = new List<MapColumnSave>();
        for (int x = 0; x < sizeM; x++)
        {
            newMSAVE.mapColumnSaves.Add(new MapColumnSave());
            newMSAVE.mapColumnSaves[x].tS = new List<TileS>();
            for (int y = 0; y < heightSize; y++)
            {
                if (groundMap[x, y].type != (int)TileType.GRASS)
                {
                    newMSAVE.mapColumnSaves[x].tS.Add(new TileS
                    {
                        placeable = groundMap[x, y].placeable,
                        position = new Vector2Int(x, y),
                        type = groundMap[x, y].type
                    });
                }
            }
        }

        NodePathLevelSave levelData = new NodePathLevelSave
        {
            mSave = newMSAVE,
            tracksSave = tracks,
            waveSave = waveList,
            spawnTime = spawnT,
            breakTime = breakT,
            size = new Vector2Int(sizeM, heightSize),
            nodePathVersion = 1,
            nodePaths = nodePaths.Copy(),
            pathGround = new List<NodePathGround>(pathGround)
        };

        saveName = CleanFileName(saveName) + ".json";
        string json = JsonUtility.ToJson(levelData, false);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, saveName), json);
    }

    // call AFTER your loader has created groundMap and restored the saved terrain
    // this restores the editor graph only; your loader still owns waves and other data
    public bool LoadNodePathsFromJSON(string json)
    {
        NodePathLevelSave data;
        try
        {
            data = JsonUtility.FromJson<NodePathLevelSave>(json);
        }
        catch (ArgumentException)
        {
            SetPathMessage("Could not read the level JSON.");
            return false;
        }

        if (data == null || data.nodePathVersion != 1 || data.nodePaths == null || data.pathGround == null)
        {
            SetPathMessage("This save has no supported node editor data. Older routes can still be used for gameplay.");
            return false;
        }
        if (groundMap == null || data.size.x != groundMap.GetLength(0) || data.size.y != groundMap.GetLength(1))
        {
            SetPathMessage("Create the saved map size and restore its terrain before loading the nodes.");
            return false;
        }

        string message;
        if (!data.nodePaths.Validate(data.size.x, data.size.y, true, out message))
        {
            SetPathMessage(message);
            return false;
        }
        List<List<Vector3Int>> savedRoutes;
        if (!data.nodePaths.BuildRoutes(out savedRoutes, out message))
        {
            SetPathMessage(message);
            return false;
        }

        // every covered tile needs exactly one original terrain record
        HashSet<Vector3Int> covered = new HashSet<Vector3Int>();
        foreach (NodePathNode node in data.nodePaths.nodes)
            covered.Add(node.position);
        foreach (NodePathConnection connection in data.nodePaths.connections)
        {
            foreach (Vector3Int cell in connection.cells)
                covered.Add(cell);
        }
        HashSet<Vector3Int> savedGround = new HashSet<Vector3Int>();
        foreach (NodePathGround tile in data.pathGround)
        {
            if (tile == null || !covered.Contains(tile.position) || !savedGround.Add(tile.position) ||
                tile.type < 0 || tile.type >= tiles.Count || tile.type == (int)TileType.PATH)
            {
                SetPathMessage("The saved terrain underneath the paths is invalid.");
                return false;
            }
        }
        if (!covered.SetEquals(savedGround))
        {
            SetPathMessage("The save is missing terrain underneath a path.");
            return false;
        }
        for (int x = 0; x < data.size.x; x++)
        {
            for (int y = 0; y < data.size.y; y++)
            {
                TileC tile = groundMap[x, y];
                if (tile == null || (tile.type == (int)TileType.PATH && !covered.Contains(new Vector3Int(x, y, 0))))
                {
                    SetPathMessage("Restore the matching saved terrain before loading these nodes.");
                    return false;
                }
            }
        }

        StopPlacement();
        sizeM = data.size.x;
        heightSize = data.size.y;
        nodePaths = data.nodePaths;
        pathGround = data.pathGround;
        mapReady = true;
        RebuildNodeTiles();
        GenerateNodeRoutes();
        UpdateNodeEndButton();
        return true;
    }

    private string CleanFileName(string input)
    {
        string cleanedName = input;
        if (!string.IsNullOrEmpty(cleanedName))
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            cleanedName = new string(input.Where(c => !invalidChars.Contains(c)).ToArray());
            cleanedName = cleanedName.Trim();
        }
        if (string.IsNullOrEmpty(cleanedName))
            cleanedName = "LevelSave" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        return cleanedName;
    }
}

// extra editor fields live alongside your existing gameplay fields
// no changes to your project's existing LevelData definition are needed
[Serializable]
public class NodePathLevelSave
{
    public MapSave mSave;
    public List<Routes> tracksSave;
    public List<Waves> waveSave;
    public float spawnTime;
    public float breakTime;
    public Vector2Int size;
    public int nodePathVersion;
    public NodePathGraph nodePaths;
    public List<NodePathGround> pathGround;
}
