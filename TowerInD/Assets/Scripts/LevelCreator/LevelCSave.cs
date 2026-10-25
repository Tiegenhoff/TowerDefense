using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
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
                newMSAVE.mapColumnSaves[x].tS.Add(new TileS());
                
                // copy the data
                newMSAVE.mapColumnSaves[x].tS[y].placeable = groundMap[x, y].placeable;
                newMSAVE.mapColumnSaves[x].tS[y].position = groundMap[x, y].position;
                newMSAVE.mapColumnSaves[x].tS[y].type = groundMap[x, y].type;
            }
        }

        // create the level save data
        LevelData levelData = new LevelData
        {
            mSave = newMSAVE,
            tracksSave = tracks,
            waveSave = waveList
        };

        // save the data
        saveName = CleanFileName(saveName) + ".json";

        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, saveName), json);
    }

    // to clean up name
    private string CleanFileName(string input)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        string cleanedName = new string(input.Where(c => !invalidChars.Contains(c)).ToArray());

        cleanedName = cleanedName.Trim();

        if (string.IsNullOrEmpty(cleanedName))
            cleanedName = "LevelSave" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        return cleanedName;
    }
}
