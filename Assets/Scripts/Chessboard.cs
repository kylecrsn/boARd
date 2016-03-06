using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Chessboard : NetworkBehaviour
{
    public override void OnStartClient()
    {
        if (!isClient)
            return;

        var materialWood = (Material)Resources.Load("wood");

        this.GetComponent<MeshRenderer>().material = materialWood;
    }
}