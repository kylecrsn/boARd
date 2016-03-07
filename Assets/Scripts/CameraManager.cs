using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    private WebCamTexture cameraBack;
    private GameObject cameraBlur;

    //Called when the scene first starts
    public void Start()
    {
        //Set the size of the rect to match the canvas (and thus the user's screen)
        this.GetComponent<RectTransform>().sizeDelta = new Vector2(
            GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>().rect.height,
            GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>().rect.width);
        cameraBlur = GameObject.Find("CameraBlur");
        cameraBlur.GetComponent<RectTransform>().sizeDelta = new Vector2(
            GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>().rect.height,
            GameObject.FindGameObjectWithTag("Canvas").GetComponent<RectTransform>().rect.width);
        //Initialize the camera and the raw image's texture
        cameraBack = new WebCamTexture();
        this.GetComponent<RawImage>().texture = cameraBack;
        this.GetComponent<RawImage>().material.mainTexture = cameraBack;
        cameraBack.Play();
    }

    void Update()
    {
        //Wait for the correct camera infoto be obtained from the hardware
        if (cameraBack.width < 100)
            return;

        //Obtain the rotation of the screen, handling when the image has been mirrored and setting the raw image's rotation based off this
        var deviceRotation = -cameraBack.videoRotationAngle;
        if (cameraBack.videoVerticallyMirrored)
            deviceRotation += 180;
        this.GetComponent<RawImage>().rectTransform.localEulerAngles = new Vector3(0f, 0f, deviceRotation);
        cameraBlur.GetComponent<RectTransform>().localEulerAngles = new Vector3(0f, 0f, deviceRotation);

        //Set the aspect ratio to match the new rotation
        this.GetComponent<RawImage>().GetComponent<AspectRatioFitter>().aspectRatio = cameraBack.width / cameraBack.height;
        cameraBlur.GetComponent<RawImage>().GetComponent<AspectRatioFitter>().aspectRatio = cameraBack.width / cameraBack.height;

        //Flip the uvRect rendered by the raw image if the image was mirrored
        if (cameraBack.videoVerticallyMirrored)
        {
            this.GetComponent<RawImage>().uvRect = new Rect(1, 0, -1, 1);
            cameraBlur.GetComponent<RawImage>().uvRect = new Rect(1, 0, -1, 1);
        }
        else
        {
            this.GetComponent<RawImage>().uvRect = new Rect(0, 0, 1, 1);
            cameraBlur.GetComponent<RawImage>().uvRect = new Rect(0, 0, 1, 1);
        }
    }
}