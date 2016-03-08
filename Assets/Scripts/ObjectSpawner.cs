using UnityEngine;
using UnityEngine.Networking;

public class ObjectSpawner : NetworkBehaviour
{
    public GameObject lightingPrefab;
    public GameObject chessboardPrefab;
    public GameObject tilePrefab;
    public GameObject checkerPrefab;

    //Called when the server is first initialized by the host
    public override void OnStartServer()
    {
        var goImageTarget = GameObject.Find("ImageTarget");
        int x, y;

        //Spawn the lighting above the board
        var goLighting = (GameObject)Instantiate(lightingPrefab, Vector3.zero, Quaternion.identity);
        goLighting.GetComponent<Transform>().parent = goImageTarget.GetComponent<Transform>();
        goLighting.GetComponent<Transform>().position = new Vector3(0f, 10f, 0f);
        goLighting.GetComponent<Transform>().eulerAngles = Quaternion.Euler(85f, 235f, 0f).eulerAngles;
        goLighting.GetComponent<Transform>().localScale = new Vector3(1f, 1f, 1f);

        NetworkServer.Spawn(goLighting);

        //Spawn the chessboard on top of the image target
        var goChessboard = (GameObject)Instantiate(chessboardPrefab, Vector3.zero, Quaternion.identity);
        goChessboard.GetComponent<Transform>().parent = goImageTarget.transform;
        goChessboard.GetComponent<Transform>().position = new Vector3(0f, 0.5f, 0f);
        goChessboard.GetComponent<Transform>().eulerAngles = Quaternion.Euler(-90f, -90f, 0f).eulerAngles;
        goChessboard.GetComponent<Transform>().localScale = new Vector3(0.1f, 0.1f, 0.1f);
        
        NetworkServer.Spawn(goChessboard);

        //Obtain original chessboard tile transforms as reference for the networked tiles
        var tiles = goChessboard.GetChildren(8, 8);

        for (x = 0; x < 8; x++)
        {
            for (y = 0; y < 8; y++)
            {
                //Spawn networked tiles with original tile transforms
                var goTile = (GameObject)Instantiate(tilePrefab, Vector3.zero, Quaternion.identity);
                goTile.GetComponent<Transform>().position = tiles[x][y].GetComponent<Transform>().position;
                goTile.GetComponent<Transform>().eulerAngles = tiles[x][y].GetComponent<Transform>().eulerAngles;
                goTile.GetComponent<Transform>().localScale = tiles[x][y].GetComponent<Transform>().localScale;
                goTile.GetComponent<Transform>().parent = GameObject.Find("ChessboardReference").GetComponent<Transform>();
                goTile.GetComponent<Tile>().gridX = x;
                goTile.GetComponent<Tile>().gridY = y;
                goTile.GetComponent<Tile>().white = false;
                goTile.GetComponent<Tile>().valid = false;
                goTile.GetComponent<Tile>().redChecker = false;
                goTile.GetComponent<Tile>().blackChecker = false;
                
                //Spawn checker pieces relative to the surface of each tile
                if ((x % 2 == 0 && y % 2 == 0 && (y == 0 || y == 2 || y == 6)) || (x % 2 == 1 && y % 2 == 1 && (y == 1 || y == 5 || y == 7)))
                {
                    var goChecker = (GameObject)Instantiate(checkerPrefab, Vector3.zero, Quaternion.identity);
                    goChecker.GetComponent<Transform>().parent = goImageTarget.transform;
                    goChecker.GetComponent<Transform>().position = new Vector3(
                        goTile.GetComponent<Transform>().position.x,
                        1.133f * goTile.GetComponent<Transform>().position.y,
                        goTile.GetComponent<Transform>().position.z);
                    goChecker.GetComponent<Transform>().eulerAngles = Quaternion.Euler(-90f, 0f, 0f).eulerAngles;
                    goChecker.GetComponent<Transform>().localScale = new Vector3(0.09f, 0.09f, 0.09f);
                    goChecker.GetComponent<Checker>().originPos = goChecker.GetComponent<Transform>().position;
                    if (y < 4)
                    {
                        goChecker.GetComponent<Checker>().red = false;
                        goTile.GetComponent<Tile>().blackChecker = true;
                    }
                    else
                    {
                        goChecker.GetComponent<Checker>().red = true;
                        goTile.GetComponent<Tile>().redChecker = true;
                    }

                    NetworkServer.Spawn(goChecker);
                }
                else if ((x % 2 == 1 && y % 2 == 0) || (x % 2 == 0 && y % 2 == 1))
                    goTile.GetComponent<Tile>().white = true;
//                else
//                    goTile.GetComponent<Tile>().valid = true;

                NetworkServer.Spawn(goTile);
            }
        }
    }
}
