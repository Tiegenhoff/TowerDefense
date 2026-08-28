using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelC : MonoBehaviour
{
    public int size = 18;           //Pick Map Size
    public Tilemap groundTilemap;   //Base Level Ground Tilemap

    public List<AutoTile> tiles;    //List Of Tiles, Use Enum TileType

    public TileC start;             //Starting Tile AT EDGE
    public TileC last;              //Ending Tile AT EDGE

    public TileC[][] groundMap;     //2D Array Containing The Tiles

    void Start()
    {
        //Make sure the size is proper
        if (size < 18)
            size = 18;

        if (size % 2 != 0)
            size--;



        //groundTilemap.SetTile(new Vector3Int(0, 0, 0), tiles[(int)TileType.GRASS]);

        StartCoroutine(PlaceTiles());
        
        
    }

    public IEnumerator PlaceTiles()
    {
        int halfSize = size / 2;

        for (int x = halfSize * -1; x < halfSize; x++)
        {
            for (int y = halfSize * -1; y < halfSize; y++)
            {

                groundTilemap.SetTile(new Vector3Int(x, y, 0), tiles[(int)TileType.GRASS]);
                yield return new WaitForSeconds(0.02f);
            }
        }
    }
}
