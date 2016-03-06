using UnityEngine;
using UnityEngine.Networking;

public class Checker : NetworkBehaviour
{
    [SyncVar]
    public Vector3 position;
    [SyncVar]
    public Vector3 rotation;
    [SyncVar]
    public Vector3 scale;
    [SyncVar]
    public bool isBlack;

    public override void OnStartClient()
    {
        if (!isClient)
            return;

        var goImageTarget = GameObject.Find("ImageTarget");
        var materialPlasticBlack = (Material)Resources.Load("plastic-black");
        var materialPlasticRed = (Material)Resources.Load("plastic-red");

        this.GetComponent<Transform>().parent = goImageTarget.transform;
        this.GetComponent<Transform>().position = position;
        this.GetComponent<Transform>().eulerAngles = rotation;
        this.GetComponent<Transform>().localScale = scale;
        if (this.isBlack == true)
            this.GetComponent<MeshRenderer>().material = materialPlasticBlack;
        else
            this.GetComponent<MeshRenderer>().material = materialPlasticRed;
    }
}