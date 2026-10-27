using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public partial class LevelC : MonoBehaviour
{
    
    
    
    // save level to JSON
    public void SaveToJSON(string saveName) // should change later to differentiate levels from story and player made levels (or at least move the story levels to a different folder after)
    {
        // setup the save for groundMap (Convert it into saveable data)
        MapSave newMSAVE = new MapSave();
        newMSAVE.mapColumnSaves = new List<MapColumnSave>();
        
        for (int x = 0; x < sizeM; x++)
        {
            newMSAVE.mapColumnSaves.Add(new MapColumnSave());
            newMSAVE.mapColumnSaves[x].tS = new List<TileS>();
            for (int y = 0; y < heightSize; y++)
            {
                // if GRASS skip over for the sake of file size
                if (groundMap[x, y].type != (int)TileType.GRASS) // because of this, when loading levels, position will have to be relied on
                {
                    // copy the data
                    newMSAVE.mapColumnSaves[x].tS.Add(new TileS
                    {
                        placeable = groundMap[x, y].placeable,
                        position = new Vector2Int(x, y),
                        type = groundMap[x, y].type
                    });
                }
            }
        }

        // create the level save data
        LevelData levelData = new LevelData
        {
            mSave = newMSAVE,
            tracksSave = tracks,
            waveSave = waveList,
            spawnTime = spawnT,
            breakTime = breakT,
            size = new Vector2Int(sizeM, heightSize)
        };

        // save the data
        saveName = CleanFileName(saveName) + ".json";

        string json = JsonUtility.ToJson(levelData, false);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, saveName), json);
    }

    // to clean up name
    private string CleanFileName(string input)
    {
        string cleanedName = input;

        if (!string.IsNullOrEmpty(cleanedName))
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();
            cleanedName = new string(input.Where(c => !invalidChars.Contains(c)).ToArray());

            cleanedName = cleanedName.Trim();
        }

        // double check after cleaning
        if (string.IsNullOrEmpty(cleanedName))
            cleanedName = "LevelSave" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        return cleanedName;
    }
}
