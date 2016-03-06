using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
public class WebCamBehavior : MonoBehaviour
{
    /// <summary>
    /// Meta reference to the camera
    /// </summary>

    public RawImage rawimage;
    private WebCamTexture webcamTexture;
    /// <summary>
    /// The number of frames per second
    /// </summary>
    private int m_framesPerSecond = 0;

    /// <summary>
    /// The current frame count
    /// </summary>
    private int m_frameCount = 0;

    /// <summary>
    /// The frames timer
    /// </summary>
    private DateTime m_timerFrames = DateTime.MinValue;

    /// <summary>
    /// The selected device index
    /// </summary>
    private int m_indexDevice = -1;

    /// <summary>
    /// The web cam texture
    /// </summary>
    //private WebCamTexture m_texture = null;

    // Use this for initialization
    void Start()
    {



        Application.RequestUserAuthorization(UserAuthorization.WebCam);
    }

    void OnGUI()
    {
        if (m_timerFrames < DateTime.Now)
        {
            m_framesPerSecond = m_frameCount;
            m_frameCount = 0;
            m_timerFrames = DateTime.Now + TimeSpan.FromSeconds(1);
        }
        ++m_frameCount;

        GUILayout.Label(string.Format("Frames per second: {0}", m_framesPerSecond));

        if (m_indexDevice >= 0 && WebCamTexture.devices.Length > 0)
        {
            GUILayout.Label(string.Format("Selected Device: {0}", WebCamTexture.devices[m_indexDevice].name));
        }

        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            GUILayout.Label("Has WebCam Authorization");
            if (null == WebCamTexture.devices)
            {
                GUILayout.Label("Null web cam devices");
            }
            else
            {
                GUILayout.Label(string.Format("{0} web cam devices", WebCamTexture.devices.Length));
                for (int index = 0; index < WebCamTexture.devices.Length; ++index)
                {
                    var device = WebCamTexture.devices[index];
                    if (string.IsNullOrEmpty(device.name))
                    {
                        GUILayout.Label("unnamed web cam device");
                        continue;
                    }

                    if (GUILayout.Button(string.Format("web cam device {0}{1}{2}",
                                                       m_indexDevice == index
                                                       ? "["
                                                       : string.Empty,
                                                       device.name,
                                                       m_indexDevice == index ? "]" : string.Empty),
                                         GUILayout.MinWidth(200),
                                         GUILayout.MinHeight(50)))
                    {
                        m_indexDevice = index;

                        // stop playing
                        if (null != webcamTexture)
                        {
                            if (webcamTexture.isPlaying)
                            {
                                webcamTexture.Stop();
                            }
                        }

                        // destroy the old texture
                        if (null != webcamTexture)
                        {
                            UnityEngine.Object.DestroyImmediate(webcamTexture, true);
                        }

                        // use the device name
                        webcamTexture = new WebCamTexture(device.name, 2560, 1440);

                        // start playing
                        webcamTexture.Play();

                        // assign the texture
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
                    }
                }
            }
        }
        else
        {
            GUILayout.Label("Pending WebCam Authorization...");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (null != webcamTexture &&
            webcamTexture.didUpdateThisFrame)
        {
            // assign the texture
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
        }
    }
}