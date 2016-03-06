using UnityEngine;
using UnityEngine.Networking;

public class Tile : NetworkBehaviour
{
    [SyncVar]                       //0 -> 63 grid values == (gridX * 8) + gridY
    public int gridX;
    [SyncVar]
    public int gridY;

    public override void OnStartClient()
    {
        if (!isClient)
            return;

        var materialTileBlack = (Material)Resources.Load("tiling-black");
        var materialTileWhite = (Material)Resources.Load("tiling-white");

        if ((this.gridX % 2 == 0 && this.gridY % 2 == 0) || (this.gridX % 2 == 1 && this.gridY % 2 == 1))
            this.GetComponent<MeshRenderer>().material = materialTileBlack;
        else
            this.GetComponent<MeshRenderer>().material = materialTileWhite;
    }
}