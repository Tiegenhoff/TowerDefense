using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public partial class LevelC : MonoBehaviour
{
    public int sizeM = 18;           //Pick Map Size
    private int heightSize;         //Map size height
    public Tilemap groundTilemap;   //Base Level Ground Tilemap

    public List<AutoTile> tiles;    //List Of Tiles, Use Enum TileType


    [System.NonSerialized] public TileC start;             //Starting Tile AT EDGE
    [System.NonSerialized] public TileC last;              //Ending Tile AT EDGE
    [System.NonSerialized] public List<TileC> last2;       //Ending Tile AT EDGE

    [System.NonSerialized] public TileC[,] groundMap;     //2D Array Containing The Tiles


    // pathing
    [System.NonSerialized] public List<Routes> tracks;

    // spawning
    private float spawnT;
    private float breakT;
    private bool pickingSetup;

    [Header("Selection")]
    public GameObject selectionUI;
    public GameObject sizeInput;

    void Start()
    {
        string filePath = Application.persistentDataPath;
        Debug.Log(filePath);
        
        StartCoroutine(Setup());
        
    }
    public IEnumerator Setup()
    {
        // get placement setup
        yield return StartCoroutine(SelectionLoop());

        // generate and setup tiles
        StartCoroutine(PlaceTiles());
        yield return null;
    }
    

    // wait for Size Selection
    public IEnumerator SelectionLoop()
    {
        selectionUI.SetActive(true);
        
        // make sure selection starts at 0
        sizeM = 0;

        pickingSetup = true;
        while (pickingSetup)
        {

            yield return null;
        }

        selectionUI.SetActive(false);

        yield return null;
    }







    public IEnumerator PlaceTiles()
    {
        //setup easy tile data
        TileC gtile = new TileC();
        gtile.placeable = true;
        gtile.type = (int)TileType.GRASS;

        //setup boundries
        heightSize = (int)(sizeM * .56);

        //setup array
        groundMap = new TileC[sizeM, heightSize];

        int h = 0;
        int v = 0;
        for (int x = 0; x < sizeM; x++)
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



    // Extras
    public void CompleteSelection()
    {
        sizeM = GetInput(sizeInput);
        Debug.Log($"size: {sizeM}");

        if (sizeM > 5)
            pickingSetup = false;
    }
}
