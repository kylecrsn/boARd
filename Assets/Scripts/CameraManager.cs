using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{

    public RawImage rawimage;
    private WebCamTexture webcamTexture;

    public void Start()
    {
        //var x = new Vector2(GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>().rect.width, 
        //    GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>().rect.height);
        this.GetComponent<RectTransform>().sizeDelta = new Vector2();
        webcamTexture = new WebCamTexture();
        rawimage.texture = webcamTexture;
        rawimage.material.mainTexture = webcamTexture;
        webcamTexture.Play();
    }

    void Update()
    {

        if (webcamTexture.width < 100)
        {
            Debug.Log("Still waiting another frame for correct info...");
            Debug.Log("width is: " + webcamTexture.width.ToString());

            return;
        }

        // change as user rotates iPhone or Android:

        int cwNeeded = webcamTexture.videoRotationAngle;
        // Unity helpfully returns the _clockwise_ twist needed
        // guess nobody at Unity noticed their product works in counterclockwise:
        int ccwNeeded = -cwNeeded;

        // IF the image needs to be mirrored, it seems that it
        // ALSO needs to be spun. Strange: but true.
        if (webcamTexture.videoVerticallyMirrored) ccwNeeded += 180;

        // you'll be using a UI RawImage, so simply spin the RectTransform
        rawimage.rectTransform.localEulerAngles = new Vector3(0f, 0f, ccwNeeded);

        float videoRatio = (float)webcamTexture.width / (float)webcamTexture.height;
        AspectRatioFitter rawImageARF = rawimage.GetComponent<AspectRatioFitter>();
        rawImageARF.aspectRatio = videoRatio;

        if (webcamTexture.videoVerticallyMirrored)
            rawimage.uvRect = new Rect(1, 0, -1, 1);  // means flip on vertical axis
        else
            rawimage.uvRect = new Rect(0, 0, 1, 1);  // means no flip

        Debug.Log("videoRatio: " + videoRatio.ToString());


    }
}