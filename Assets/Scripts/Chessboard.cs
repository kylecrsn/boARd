using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class Chessboard : NetworkBehaviour
{
    [SyncVar]
    public Vector3 position;
    [SyncVar]
    public Quaternion rotation;
    [SyncVar]
    public Vector3 scale;
    [SyncVar]
    public Material material;

    public override void OnStartClient()
    {
        base.OnStartClient();
        this.transform.localPosition = position;
        this.transform.localRotation = rotation;
        this.transform.localScale = scale;
        this.GetComponent<Renderer>().material = material;
    }
}
