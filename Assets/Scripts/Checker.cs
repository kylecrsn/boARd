using UnityEngine;
using UnityEngine.Networking;

public class Checker : NetworkBehaviour
{
    [SyncVar]
    public bool isBlack;

    public override void OnStartClient()
    {
        if (!isClient)
            return;

        var materialPlasticBlack = (Material)Resources.Load("plastic-black");
        var materialPlasticRed = (Material)Resources.Load("plastic-red");

        if (this.isBlack == true)
            this.GetComponent<MeshRenderer>().material = materialPlasticBlack;
        else
            this.GetComponent<MeshRenderer>().material = materialPlasticRed;
    }
}