using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    //public TileC[,] groundMapSave;
    public MapSave mSave;
    public List<Routes> tracksSave;
    public List<Waves> waveSave;

    public float spawnTime;
    public float breakTime;

    public Vector2Int size;
}



[System.Serializable]
public class MapSave
{
    public List<MapColumnSave> mapColumnSaves;
}

[System.Serializable]
public class MapColumnSave
{
    public List<TileS> tS;
}

[System.Serializable]

public class TileS
{
    public bool placeable;      //Can Units Be Placed On Me?
    public Vector2Int position; //Position in groundMap
    public int type;            //(int)TileType.GRASS etc
}