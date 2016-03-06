using UnityEngine;
using UnityEngine.Networking;

public class Checker : NetworkBehaviour
{
    [SyncVar] //True if the checker is red, false if it is black
    public bool red;
    [SyncVar] //True if the tile below the checker is white, false if it is black
    public bool white;
    [SyncVar] //Original position of the checker when it was picked up, used in calculating valid move locations
    public Vector3 originPos;

    //Called when the client connects to the server, after its SyncVars have been initialized
    public override void OnStartClient()
    {
        if (!isClient)
            return;

        var materialPlasticBlack = (Material)Resources.Load("plastic-black");
        var materialPlasticRed = (Material)Resources.Load("plastic-red");

        //Assign the checker's material
        if (this.red == true)
            this.GetComponent<MeshRenderer>().material = materialPlasticRed;
        else
            this.GetComponent<MeshRenderer>().material = materialPlasticBlack;
    }
}