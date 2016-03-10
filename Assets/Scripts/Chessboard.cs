using UnityEngine;
using UnityEngine.Networking;

public class Chessboard : NetworkBehaviour
{
    [SyncVar] //Server's (black) turn when true, client's (red) turn when false
    public bool turn;

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