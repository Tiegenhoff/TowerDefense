using System.IO;
using UnityEngine;

public class LevelPlay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // load the save data first
        LevelData save = Load(LevelDirectory.fileName);



    }




    public LevelData Load(string fileName)
    {
        string filePath = Path.Combine(Application.dataPath, fileName);

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
