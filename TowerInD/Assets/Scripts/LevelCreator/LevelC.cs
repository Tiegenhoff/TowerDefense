using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public partial class LevelC : MonoBehaviour
{
    public int size = 18;           //Pick Map Size
    public Tilemap groundTilemap;   //Base Level Ground Tilemap

    public List<AutoTile> tiles;    //List Of Tiles, Use Enum TileType


    [System.NonSerialized] public TileC start;             //Starting Tile AT EDGE
    [System.NonSerialized] public TileC last;              //Ending Tile AT EDGE
    [System.NonSerialized] public List<TileC> last2;       //Ending Tile AT EDGE

    [System.NonSerialized] public TileC[,] groundMap;     //2D Array Containing The Tiles


    // pathing
    [System.NonSerialized] public List<Routes> tracks;

    void Start()
    {
        //Make sure the size is proper
        /*
        if (size < 18)
            size = 18;

        if (size % 2 != 0)
            size--;
        */


        //groundTilemap.SetTile(new Vector3Int(0, 0, 0), tiles[(int)TileType.GRASS]);

        StartCoroutine(PlaceTiles());
        


    }

    public IEnumerator PlaceTiles()
    {
        //setup easy tile data
        TileC gtile = new TileC();
        gtile.placeable = true;
        gtile.type = (int)TileType.GRASS;

        //setup boundries
        int heightSize = (int)(size * .56);

        //setup array
        groundMap = new TileC[size, heightSize];

        int h = 0;
        int v = 0;
        for (int x = 0; x < size; x++)
        {
            v = 0;
            for (int y = 0; y < heightSize; y++)
            {
                Debug.Log($"{h}, {v}");
                groundMap[h,v] = gtile;
                groundTilemap.SetTile(new Vector3Int(x, y, 0), tiles[(int)TileType.GRASS]);

                v++;
                yield return new WaitForSeconds(0.002f);
            }
            h++;
        }
    }
}
