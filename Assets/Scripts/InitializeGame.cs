using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class InitializeGame : NetworkBehaviour {

    public GameObject imageTargetPrefab;
    public GameObject chessboardPrefab;
    public GameObject checkerPrefab;
    public GameObject lightPrefab;
    public GameObject cameraPrefab;
    public Material woodMat;
    public Material tileBlackMat;
    public Material tileWhiteMat;
    public Material plasticBlackMat;
    public Material plasticRedMat;
    public Texture imageTargetTex;

    private const float _squareFactor = 1.83706f;
    private const float _boundFactor = 6.4297f;
    private const float _yFactor = 0.074f;
    
    public override void OnStartServer()
    {
        if (!isServer)
            return;

        int i;

        base.OnStartServer();

        //Spawn the image target
        var posImageTarget = new Vector3(0f, 0f, 0f);
        var rotImageTarget = Quaternion.Euler(0f, 0f, 0f);
        var scaImageTarget = new Vector3(10f, 10f, 10f);
        var imageTargetMaterial = new Material(Shader.Find("Unlit/Texture"));
        imageTargetMaterial.mainTexture = imageTargetTex;

        var goImageTarget = (GameObject)Instantiate(imageTargetPrefab, posImageTarget, rotImageTarget);
        var imageTarget = goImageTarget.GetComponent<ImageTarget>();
        imageTarget.material = imageTargetMaterial;
        imageTarget.scale = scaImageTarget;

        NetworkServer.Spawn(goImageTarget);

        //Spawn the chessboard
        var posChessboard = new Vector3(0f, 0.05f, 0f);
        var rotChessboard = Quaternion.Euler(-90f, 0f, -90f);
        var scaChessboard = new Vector3(0.1f, 0.1f, 0.1f);
        var goChessboard = (GameObject)Instantiate(chessboardPrefab, posChessboard, rotChessboard);
        var chessboard = goChessboard.GetComponent<Chessboard>();

        chessboard.material = woodMat;
        chessboard.scale = scaChessboard;
        goChessboard.transform.parent = goImageTarget.transform;

        List<GameObject> squares = goChessboard.GetChildren();
        int tileShift = 0;
        for (i = 0; i < 64; i++)
        {
            if (i % 8 == 0)
                tileShift = 1 - tileShift;

            if ((i + tileShift) % 2 == 0)
                squares[i].GetComponent<Renderer>().material = tileWhiteMat;
            else
                squares[i].GetComponent<Renderer>().material = tileBlackMat;
        }

        NetworkServer.Spawn(goChessboard);

        //Spawn pieces
        var xShiftPiece = 0;

        for(i = 0; i < 12; i++)
        {
            if (i % 4 == 0 && i > 0)
                xShiftPiece = 1 - xShiftPiece;

            var posChecker = new Vector3(_boundFactor - ((i % 4) * 2 * _squareFactor) - (xShiftPiece * _squareFactor), _yFactor, _boundFactor - ((i / 4) * _squareFactor));
            var rotChecker = Quaternion.Euler(-90f, 0f, 0f);
            var scaChecker = new Vector3(0.09f, 0.09f, 0.09f);

            var goRed = (GameObject)Instantiate(checkerPrefab, posChecker, rotChecker);
            var checkerRed = goRed.GetComponent<Checker>();
            checkerRed.material = plasticRedMat;
            checkerRed.scale = scaChecker;
            goRed.transform.parent = goImageTarget.transform;

            posChecker.x = -posChecker.x;
            posChecker.z = -posChecker.z;
            var goBlack = (GameObject)Instantiate(checkerPrefab, posChecker, rotChecker);
            var checkerBlack = goBlack.GetComponent<Checker>();
            checkerBlack.material = plasticBlackMat;
            checkerBlack.scale = scaChecker;
            goBlack.transform.parent = goImageTarget.transform;

            NetworkServer.Spawn(goRed);
            NetworkServer.Spawn(goBlack);
        }

        //Spawn lighting
        var posLight = new Vector3(2f, 5f, 2f);
        var rotLight = Quaternion.Euler(50f, -135f, 0f);

        var goLight = (GameObject)Instantiate(lightPrefab, posLight, rotLight);
        goLight.transform.parent = goImageTarget.transform;

        NetworkServer.Spawn(goLight);

        //Spawn Camera
        var posCamera = new Vector3(0f, 20f, 0f);
        var rotCamera = Quaternion.Euler(90f, 0f, 0f);

        var camera = (GameObject)Instantiate(cameraPrefab, posCamera, rotCamera);

        NetworkServer.Spawn(camera);
    }
}
