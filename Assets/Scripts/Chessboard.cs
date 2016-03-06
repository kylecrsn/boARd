using UnityEngine;
using UnityEngine.Networking;

public class Chessboard : NetworkBehaviour
{
    //Called when the client connects to the server, after its SyncVars have been initialized
    public override void OnStartClient()
    {
        if (!isClient)
            return;

        var materialWood = (Material)Resources.Load("wood");

        //Assign the chessboard frame's material
        this.GetComponent<MeshRenderer>().material = materialWood;
    }
}