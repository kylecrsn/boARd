using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class ObjectSpawner : NetworkBehaviour
{
    public GameObject lightingPrefab;
    public GameObject chessboardPrefab;
    public GameObject tilePrefab;
    public GameObject checkerPrefab;

    public override void OnStartServer()
    {
        int i, j;
        var goImageTarget = GameObject.Find("ImageTarget");

        //Spawn the lighting above the board
        var goLighting = (GameObject)Instantiate(lightingPrefab, Vector3.zero, Quaternion.identity);
        goLighting.GetComponent<Transform>().parent = goImageTarget.GetComponent<Transform>();
        goLighting.GetComponent<Transform>().position = new Vector3(0f, 10f, 0f);
        goLighting.GetComponent<Transform>().eulerAngles = Quaternion.Euler(85f, 235f, 0f).eulerAngles;
        goLighting.GetComponent<Transform>().localScale = new Vector3(1f, 1f, 1f);

        NetworkServer.Spawn(goLighting);

        //Spawn the chessboard slightly above origin
        var goChessboard = (GameObject)Instantiate(chessboardPrefab, Vector3.zero, Quaternion.identity);
        goChessboard.GetComponent<Transform>().parent = goImageTarget.transform;
        goChessboard.GetComponent<Transform>().position = new Vector3(0f, 0.5f, 0f);
        goChessboard.GetComponent<Transform>().eulerAngles = Quaternion.Euler(-90f, -90f, 0f).eulerAngles;
        goChessboard.GetComponent<Transform>().localScale = new Vector3(0.1f, 0.1f, 0.1f);
        
        NetworkServer.Spawn(goChessboard);

        //Spawn networked tiles with original tile transforms, delete original tiles in chessboard client start
        var tiles = goChessboard.GetChildren(8, 8);

        for (i = 0; i < 8; i++)
        {
            for (j = 0; j < 8; j++)
            {
                var goTile = (GameObject)Instantiate(tilePrefab, Vector3.zero, Quaternion.identity);
                goTile.GetComponent<Transform>().position = tiles[i][j].GetComponent<Transform>().position;
                goTile.GetComponent<Transform>().eulerAngles = tiles[i][j].GetComponent<Transform>().eulerAngles;
                goTile.GetComponent<Transform>().localScale = tiles[i][j].GetComponent<Transform>().localScale;
                goTile.GetComponent<Transform>().parent = GameObject.Find("ChessboardReference").GetComponent<Transform>();

                var tile = goTile.GetComponent<Tile>();
                tile.gridX = i;
                tile.gridY = j;

                NetworkServer.Spawn(goTile);

                //Spawn checker pieces relative to the surface of each tile
                if ((i % 2 == 0 && j % 2 == 0 && (j == 0 || j == 2 || j == 6)) || (i % 2 == 1 && j % 2 == 1 && (j == 1 || j == 5 || j == 7)))
                {
                    var goChecker = (GameObject)Instantiate(checkerPrefab, Vector3.zero, Quaternion.identity);
                    goChecker.GetComponent<Transform>().parent = goImageTarget.transform;
                    goChecker.GetComponent<Transform>().position = new Vector3(
                        goTile.GetComponent<Transform>().position.x, 
                        1.133f * goTile.GetComponent<Transform>().position.y,
                        goTile.GetComponent<Transform>().position.z);
                    goChecker.GetComponent<Transform>().eulerAngles = Quaternion.Euler(-90f, 0f, 0f).eulerAngles;
                    goChecker.GetComponent<Transform>().localScale = new Vector3(0.09f, 0.09f, 0.09f);

                    var checker = goChecker.GetComponent<Checker>();
                    if (j < 4)
                        checker.isBlack = true;
                    else
                        checker.isBlack = false;

                    NetworkServer.Spawn(goChecker);
                }
            }
        }
    }
}
