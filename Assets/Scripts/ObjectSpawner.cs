using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class ObjectSpawner : NetworkBehaviour
{
    public GameObject lightingPrefab;
    public GameObject chessboardPrefab;
    public GameObject checkerPrefab;

    private const float _squareFactor = 0.183706f;
    private const float _boundFactor = 0.64297f;
    private const float _yFactor = 0.074f;

    public override void OnStartServer()
    {
        int i, j;

        //Spawn the lighting above the board
        var goLighting = (GameObject)Instantiate(lightingPrefab, Vector3.zero, Quaternion.identity);
        var lighting = goLighting.GetComponent<Lighting>();
        var posLighting = new Vector3(0f, 10f, 0f);
        var rotLighting = Quaternion.Euler(85f, 235f, 0f);
        var scaLighting = new Vector3(1f, 1f, 1f);

        lighting.position = posLighting;
        lighting.rotation = rotLighting.eulerAngles;
        lighting.scale = scaLighting;

        NetworkServer.Spawn(goLighting);

        //Spawn the chessboard slightly above origin
        var goChessboard = (GameObject)Instantiate(chessboardPrefab, Vector3.zero, Quaternion.identity);
        var chessboard = goChessboard.GetComponent<Chessboard>();
        var posChessboard = new Vector3(0f, 0.5f, 0f);
        var rotChessboard = Quaternion.Euler(-90f, -90f, 0f);
        var scaChessboard = new Vector3(0.1f, 0.1f, 0.1f);

        chessboard.position = posChessboard;
        chessboard.rotation = rotChessboard.eulerAngles;
        chessboard.scale = scaChessboard;
        chessboard.tiles = gameObject.GetChildren(8, 8);

        NetworkServer.Spawn(goChessboard);

        //Spawn checker pieces relative to the surface of the tiles
        for(i = 0; i < 8; i++)
        {
            for(j = 0; j < 8; j++)
            {
                if((i % 2 == 0 && j % 2 == 0 && (j == 0 || j == 2 || j == 6)) || (i % 2 == 1 && j % 2 == 1 && (j == 1 || j == 5 || j == 7)))
                {
                    var goChecker = (GameObject)Instantiate(checkerPrefab, Vector3.zero, Quaternion.identity);
                    var checker = goChecker.GetComponent<Checker>();
                    var posChecker = new Vector3(chessboard.tiles[i][j].GetComponent<Transform>().position.x,
                        1.133f * chessboard.tiles[i][j].GetComponent<Transform>().position.y,
                        chessboard.tiles[i][j].GetComponent<Transform>().position.z);
                    var rotChecker = Quaternion.Euler(-90f, 0f, 0f);
                    var scaChecker = new Vector3(0.09f, 0.09f, 0.09f);

                    checker.position = posChecker;
                    checker.rotation = rotChecker.eulerAngles;
                    checker.scale = scaChecker;
                    NetworkServer.Spawn(goChecker);
                }
            }
        }
    }
}
