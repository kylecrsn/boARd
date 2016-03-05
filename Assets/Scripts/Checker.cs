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

    public override void OnStartClient()
    {
        if (!isClient)
            return;

        var goImageTarget = GameObject.Find("ImageTarget");
        var materialPlasticRed = (Material)Resources.Load("plastic-red");
        var materialPlasticBlack = (Material)Resources.Load("plastic-black");

        this.GetComponent<Transform>().parent = goImageTarget.transform;
        this.GetComponent<Transform>().position = position;
        this.GetComponent<Transform>().eulerAngles = rotation;
        this.GetComponent<Transform>().localScale = scale;
        if(this.GetComponent<Transform>().position.z > 0)
            this.GetComponent<MeshRenderer>().material = materialPlasticRed;
        else
            this.GetComponent<MeshRenderer>().material = materialPlasticBlack;
    }
}