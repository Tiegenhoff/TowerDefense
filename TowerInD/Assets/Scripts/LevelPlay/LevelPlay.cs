using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelPlay : MonoBehaviour
{
    // level parameters
    private TileC[,] groundMap;
    private List<Routes> tracksSave;
    private List<Waves> waveSave;
    private float spawnTime;
    private float breakTime;
    // we'll add specific units later

    [Header("TileMap")]
    public Tilemap groundTilemap;
    public List<AutoTile> tiles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // load the save data first
        LevelData save = Load(LevelDirectory.fileName);

        if (save == null)
        {
            Debug.Log($"[!!!] Load Failure!");
            return;
        }

        // unravel the data
        TranslateData(save);

        // generate level (calls the next step at the end)
        StartCoroutine(PlaceTiles());
    }

    public IEnumerator PlaceTiles()
    {
        for (int x = 0; x < groundMap.GetLength(0); x++)
        {
            for (int y = 0; y < groundMap.GetLength(1); y++)
            {
                groundTilemap.SetTile(new Vector3Int(x, y, 0), tiles[groundMap[x, y].type]);

                yield return new WaitForSeconds(0.002f);
            }
        }

        // once done start play (maybe make it based off placements
    }


    private void TranslateData(LevelData save)
    {
        tracksSave = save.tracksSave;
        waveSave = save.waveSave;
        spawnTime = save.spawnTime;
        breakTime = save.breakTime;

        groundMap = new TileC[save.size.x, save.size.y];
        for (int x = 0; x < save.size.x; x++)
        {
            for (int y = 0; y < save.size.y; y++)
            {
                //groundMap[x, y]
                TileC newTile = new TileC();
                newTile.placeable = true;
                newTile.type = (int)TileType.GRASS;

                // if tile exists
                foreach (TileS tile in save.mSave.mapColumnSaves[x].tS)
                {
                    if (tile.position == new Vector2Int(x, y))
                    {
                        newTile.placeable = tile.placeable;
                        newTile.type = tile.type;

                        break;
                    }
                }

                groundMap[x, y] = newTile;
            }
        }



    }


    private LevelData Load(string fileName)
    {
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(filePath))
        {
            Debug.Log($"[!!!] filePath not found: {filePath}");
            return null;
        }

        string json = File.ReadAllText(filePath);
        LevelData levelSave = JsonUtility.FromJson<LevelData>(json);

        return levelSave;
    }
}
