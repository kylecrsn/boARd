using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class Checker : NetworkBehaviour
{
    [SyncVar]
    public Material material;
    [SyncVar]
    public Vector3 scale;
    [SyncVar]
    public Vector3 position;
    [SyncVar]
    public Quaternion roatation;

    void Start()
    {
        this.transform.localScale = scale;
        this.GetComponent<Renderer>().material = material;
    }
}
