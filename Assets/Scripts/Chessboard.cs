using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Chessboard : NetworkBehaviour
{
    [SyncVar]
    public Vector3 position;
    [SyncVar]
    public Vector3 rotation;
    [SyncVar]
    public Vector3 scale;

    public List<List<GameObject>> tiles;

    public override void OnStartClient()
    {
        if (!isClient)
            return;

        var goImageTarget = GameObject.Find("ImageTarget");
        var materialWood = (Material)Resources.Load("wood");
        var materialTileBlack = (Material)Resources.Load("tiling-black");
        var materialTileWhite = (Material)Resources.Load("tiling-white");
        int i, j;

        this.GetComponent<Transform>().parent = goImageTarget.transform;
        this.GetComponent<Transform>().position = position;
        this.GetComponent<Transform>().eulerAngles = rotation;
        this.GetComponent<Transform>().localScale = scale;
        this.GetComponent<MeshRenderer>().material = materialWood;

        this.tiles = gameObject.GetChildren(8, 8);
        for(i = 0; i < 8; i++)
        {
            for(j = 0; j < 8; j++)
            {
                if((j + i % 2) % 2 == 0)
                {
                    tiles[i][j].GetComponent<MeshRenderer>().material = materialTileBlack;
                    tiles[i][j].GetComponent<Tile>().state = Tile.State.Empty;
                }
                else
                {
                    tiles[i][j].GetComponent<MeshRenderer>().material = materialTileWhite;
                    tiles[i][j].GetComponent<Tile>().state = Tile.State.Invalid;
                }
            }
        }
    }
}