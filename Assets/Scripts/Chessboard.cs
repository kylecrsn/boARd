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

    public override void OnStartClient()
    {
        if (!isClient)
            return;

        var goImageTarget = GameObject.Find("ImageTarget");
        var materialWood = (Material)Resources.Load("wood");

        this.GetComponent<Transform>().parent = goImageTarget.transform;
        this.GetComponent<Transform>().position = position;
        this.GetComponent<Transform>().eulerAngles = rotation;
        this.GetComponent<Transform>().localScale = scale;
        this.GetComponent<MeshRenderer>().material = materialWood;
    }
}