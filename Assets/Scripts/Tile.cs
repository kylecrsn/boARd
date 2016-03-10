using UnityEngine;
using UnityEngine.Networking;

public class Tile : NetworkBehaviour
{
    [SyncVar] //X coodinate of the tile in the checkerboard grid (ranges from 0 to 7)
    public int gridX;
    [SyncVar] //Y coodinate of the tile in the checkerboard grid (ranges from 0 to 7)
    public int gridY;
    [SyncVar] //True if the tile is white, false if it is black
    public bool white;
    [SyncVar] //True if the tile is a valid position to move to; includes tile color, checkerAbove value, and game rules checking
    public bool valid;
    [SyncVar] //True if the checker above the tile is red, false otherwise
    public bool redChecker;
    [SyncVar] //True if the checker above the tile is black, false otherwise
    public bool blackChecker;

    //Called when the client connects to the server, after its SyncVars have been initialized
    public override void OnStartClient()
    {
        if (!isClient)
            return;

        var materialTileWhite = (Material)Resources.Load("tiling-white");
        var materialTileBlack = (Material)Resources.Load("tiling-black");

        //Assign the tile's material
        if (this.white == true)
            this.GetComponent<MeshRenderer>().material = materialTileWhite;
        else
            this.GetComponent<MeshRenderer>().material = materialTileBlack;
    }

    //Returns a Tile GameObject from the given array of tile objects of the given x and y corrdinates
    public Tile GetTile(GameObject[] tiles, int x, int y)
    {
        foreach(var tile in tiles)
        {
            if (tile.GetComponent<Tile>().gridX == x && tile.GetComponent<Tile>().gridY == y)
                return tile.GetComponent<Tile>();
        }

        return null;
    }
}