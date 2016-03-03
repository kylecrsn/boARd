using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class InitialSpawn : NetworkBehaviour
{
    public GameObject chessboardPrefab;
    public GameObject checkerPrefab;
    public GameObject cameraPrefab;
    public Material woodMat;
    public Material tileBlackMat;
    public Material tileWhiteMat;
    public Material plasticBlackMat;
    public Material plasticRedMat;

    private const float _squareFactor = 1.83706f;
    private const float _boundFactor = 6.4297f;
    private const float _yFactor = 0.074f;

    public override void OnStartServer()
    {
        int i;

        //var goImageTarget = GameObject.Find("ImageTarget");

        //Spawn the chessboard
        var posChessboard = new Vector3(0f, 0.05f, 0f);
        var rotChessboard = Quaternion.Euler(-90f, -90f, 0f);
        var scaChessboard = new Vector3(1f, 1f, 1f);
        var goChessboard = (GameObject)Instantiate(chessboardPrefab, posChessboard, rotChessboard);
        var chessboard = goChessboard.GetComponent<Chessboard>();

        chessboard.position = posChessboard;
        chessboard.rotation = rotChessboard;
        chessboard.scale = scaChessboard;
        chessboard.material = woodMat;
        //chessboard.scale = scaChessboard;
        //goChessboard.transform.parent = goImageTarget.transform;

        //List<GameObject> squares = goChessboard.GetChildren();
        //int tileShift = 0;
        //for (i = 0; i < 64; i++)
        //{
        //    if (i % 8 == 0)
        //        tileShift = 1 - tileShift;

        //    if ((i + tileShift) % 2 == 0)
        //        squares[i].GetComponent<Renderer>().material = tileWhiteMat;
        //    else
        //        squares[i].GetComponent<Renderer>().material = tileBlackMat;
        //}

        NetworkServer.Spawn(goChessboard);

        ////Spawn pieces
        //var xShiftPiece = 0;

        //for (i = 0; i < 12; i++)
        //{
        //    if (i % 4 == 0 && i > 0)
        //        xShiftPiece = 1 - xShiftPiece;

        //    var posChecker = new Vector3(_boundFactor - ((i % 4) * 2 * _squareFactor) - (xShiftPiece * _squareFactor), _yFactor, _boundFactor - ((i / 4) * _squareFactor));
        //    var rotChecker = Quaternion.Euler(-90f, 0f, 0f);
        //    var scaChecker = new Vector3(0.09f, 0.09f, 0.09f);

        //    var goRed = (GameObject)Instantiate(checkerPrefab, posChecker, rotChecker);
        //    var checkerRed = goRed.GetComponent<Checker>();
        //    checkerRed.material = plasticRedMat;
        //    checkerRed.scale = scaChecker;
        //    //goRed.transform.parent = goImageTarget.transform;

        //    posChecker.x = -posChecker.x;
        //    posChecker.z = -posChecker.z;
        //    var goBlack = (GameObject)Instantiate(checkerPrefab, posChecker, rotChecker);
        //    var checkerBlack = goBlack.GetComponent<Checker>();
        //    checkerBlack.material = plasticBlackMat;
        //    checkerBlack.scale = scaChecker;
        //    //goBlack.transform.parent = goImageTarget.transform;

        //    NetworkServer.Spawn(goRed);
        //    NetworkServer.Spawn(goBlack);
        //}

        ////Spawn Camera
        //var posCamera = new Vector3(0f, 20f, 0f);
        //var rotCamera = Quaternion.Euler(90f, 0f, 0f);

        //var camera = (GameObject)Instantiate(cameraPrefab, posCamera, rotCamera);

        //NetworkServer.Spawn(camera);
    }
}
