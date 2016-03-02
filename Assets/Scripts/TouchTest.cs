using UnityEngine;
using System.Collections;

public class TouchTest : MonoBehaviour {

    [SerializeField]
    float _horizontalLimit = 2.5f, _verticalLimit = 2.5f, _dragSpeed = 0.1f;

    Transform pieceTran;
    Vector3 initialPos;

    // Use this for initialization
    void Start () {
        pieceTran = this.transform;
        initialPos = pieceTran.position;
	}
	
	// Update is called once per frame
	void Update () {
	    
        if(Input.touchCount == 1)
        {
            Vector2 deltaPos = Input.GetTouch(0).deltaPosition;

            switch(Input.GetTouch(0).phase)
            {
                case TouchPhase.Began:
                    break;
                case TouchPhase.Moved:
                    DragObject(deltaPos);
                    break;
                case TouchPhase.Ended:
                    break;
            }
        }
	}

    void DragObject(Vector2 deltaPos)
    {
        pieceTran.position = new Vector3(Mathf.Clamp((deltaPos.x * _dragSpeed) + pieceTran.position.x, initialPos.x - _horizontalLimit, initialPos.x + _horizontalLimit),
            Mathf.Clamp((deltaPos.y * _dragSpeed) + pieceTran.position.y, initialPos.y - _verticalLimit, initialPos.y + _verticalLimit),
            pieceTran.position.z);
    }
}