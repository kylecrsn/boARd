using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour {

	// Update is called once per frame
	void Update ()
    {
        var x = Input.GetAxis("Horizontal") * 0.1f;
        var z = Input.GetAxis("Vertical") * 0.1f;

        transform.Translate(x, 0, z);
	}
}
