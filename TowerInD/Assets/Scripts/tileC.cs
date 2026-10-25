using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;


public class TileC
{
    public bool placeable;      //Can Units Be Placed On Me?
    public Vector3 position;    //Current Position For Pathfinding
    public TileC next;          //Points to next path
    public int type;            //(int)TileType.GRASS etc

    public List<TileC> next2;   //Soon to be the next list for path splits
    public bool split;          //Path split?
}
