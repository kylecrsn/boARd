using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ImageTarget : NetworkBehaviour {

    [SyncVar]
    public Material material;
    [SyncVar]
    public Vector3 scale;

	void Start ()
    {
        this.transform.localScale = scale;
        this.GetComponent<Renderer>().material = material;
	}
}
