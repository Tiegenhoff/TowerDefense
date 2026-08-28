using UnityEngine;

public class TileC
{
    public bool placeable;      //Can Units Be Placed On Me?
    public Vector3 position;    //Current Position For Pathfinding
    public TileC next;          //Points to next path
    public int type;            //(int)TileType.GRASS etc
}
