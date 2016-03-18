using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class Player : NetworkBehaviour
{
    [SyncVar] //Checker game object that the player is currently touching/dragging
    public GameObject currentGoChecker;
    [SyncVar] //Tile game object that the current checker is sitting/floating on
    public GameObject currentGoTile;
    [SyncVar]
    public GameObject currentGoChessboard;
    [SyncVar]
    public bool switchTurns;

    public int enemypieces = 12;
    public bool isHostPlayer;
    public const float _dragSpeedFactor = 0.09f;

    //Called when the scene begins; all objects have at this point spawned into the player's client instance
    public void Start()
    {
        if (!isLocalPlayer)
            return;

        Initialization();

        switchTurns = false;
        if(ObjectSpawner.hostPlayer == true)
        {
            isHostPlayer = true;
            ObjectSpawner.hostPlayer = false;
        }
        else
        {
            isHostPlayer = false;
        }
    }

    //Called each frame of the game
    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (enemypieces <= 0)
            CmdEndGame();

        if (switchTurns == true)
        {
            currentGoChessboard = GameObject.FindGameObjectWithTag("Chessboard");
            CmdAssignObjectAuthority(currentGoChessboard.GetComponent<NetworkIdentity>().netId);
            CmdUpdatePiece(currentGoChecker, currentGoTile, currentGoChessboard);
            CmdSwitchTurns();
            CmdRemoveObjectAuthority(currentGoChessboard.GetComponent<NetworkIdentity>().netId);
            CmdUpdatePiece(currentGoChecker, currentGoTile, currentGoChessboard);
            return;
        }

        var isHostTurn = GameObject.FindGameObjectWithTag("Chessboard").GetComponent<Chessboard>().isHostTurn;

        //Return if it is not your turn
        if ((isHostPlayer == true && isHostTurn == false) || (isHostPlayer == false && isHostTurn == true))
            return;

        RaycastHit hit;

        //When one finger touches the screen...
        if (Input.touchCount == 1)
        {
            switch (Input.GetTouch(0).phase)
            {
                case TouchPhase.Began:
                    //If the touch begins on a checker, set the current checker and tile and pick up the checker
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out hit) && hit.transform.tag.Equals("Checker"))
                    {
                        //Assign currentChecker to the checker that was hit and obtain local player authority (LPA) over it
                        currentGoChecker = hit.transform.gameObject;
                        CmdAssignObjectAuthority(currentGoChecker.GetComponent<NetworkIdentity>().netId);
                        if (Physics.Raycast(currentGoChecker.GetComponent<Transform>().position, Vector3.down, out hit) && hit.transform.tag.Equals("Tile"))
                        {
                            //Assign currentTile to the tile below the checker and obtain local player authority (LPA) over it
                            currentGoTile = hit.collider.gameObject;
                            CmdAssignObjectAuthority(currentGoTile.GetComponent<NetworkIdentity>().netId);
                            CmdUpdatePiece(currentGoChecker, currentGoTile, currentGoChessboard);
                            PickPieceUp();
                            CmdUpdatePiece(currentGoChecker, currentGoTile, currentGoChessboard);
                        }
                        else
                            currentGoTile = null;
                    }
                    else
                        currentGoChecker = null;
                    break;
                case TouchPhase.Moved:
                    //If a checker is currently being touched, move it and handle all necessary gameplay logic
                    if (currentGoChecker != null && currentGoTile != null && hasAuthority == true)
                    {
                        MovePiece(Input.GetTouch(0).position);
                        CmdUpdatePiece(currentGoChecker, currentGoTile, currentGoChessboard);
                    }
                    break;
                case TouchPhase.Ended:
                    //If a checker was being touched and has now been let go, put it back down on the board and release LPA over the checker and tile
                    if (currentGoChecker != null && currentGoTile != null && hasAuthority == true)
                    {
                        PutPieceDown();
                        CmdRemoveObjectAuthority(currentGoTile.GetComponent<NetworkIdentity>().netId);
                        CmdRemoveObjectAuthority(currentGoChecker.GetComponent<NetworkIdentity>().netId);
                        CmdUpdatePiece(currentGoChecker, currentGoTile, currentGoChessboard);
                    }
                    break;
            }
        }
    }

    [Command] //Obtains local player authority (LPA) over an object; allows player to modify a non-player (server) object
    void CmdAssignObjectAuthority(NetworkInstanceId netId)
    {
        NetworkServer.objects[netId].AssignClientAuthority(connectionToClient);
    }

    [Command] //Releases local player authority (LPA) over an object; prevents player from modifying a non-player (server) object
    void CmdRemoveObjectAuthority(NetworkInstanceId netId)
    {
        NetworkServer.objects[netId].RemoveClientAuthority(connectionToClient);
    }

    [Command] //Routes an rpc call to update the piece on the client; keeps the player objects on the clients and server synced
    public void CmdUpdatePiece(GameObject goChecker, GameObject goTile, GameObject goChessboard)
    {
        //RpcUpdatePiece();
        currentGoChecker = goChecker;
        currentGoTile = goTile;
        currentGoChessboard = goChessboard;
    }

    //[Command] //Routes an rpc call to pick up a checker piece; keeps the player objects on the clients and server synced
    public void PickPieceUp()
    {
        var tiles = GameObject.FindGameObjectsWithTag("Tile");
        var checkers = GameObject.FindGameObjectsWithTag("Checker");
        var checkersCanJump = 0;
        int i, j;

        //Set cF (colorFactor), which allows all x and y grid calculations to be done regardless of player color
        //(0, 0) in bottom left
        //(7, 0) in top left
        //(0, 7) in bottom right
        //(7, 7) in top right
        int cF;
        if (isHostPlayer == true)
            cF = 1;
        else
            cF = -1;

        //Allows server to only move black pieces, client to only move red pieces
        if ((isHostPlayer == true && currentGoChecker.GetComponent<Checker>().red == false) || (isHostPlayer == false && currentGoChecker.GetComponent<Checker>().red == true))
        {
            //Handle mapping the valid tiles when hopping another piece
            for (i = 0; i < 24; i++)
            {
                bool invLeft, invRight;

                //Don't calculate jumps for checkers you can't use
                if ((isHostPlayer == true && checkers[i].GetComponent<Checker>().red == true) || (isHostPlayer == false && checkers[i].GetComponent<Checker>().red == false))
                    continue;

                //Only care about if the checker can jump and land within the grid, overwrite inv's for hopped checkers since they don't matter
                //If the returned value is null and an inv variable is true, the x and y params are outside the valid grid
                //If the returned value is null and an inv variable is false, then that tile is empty since no checker is there
                Checker hoppedLeft = Checker.GetChecker(checkers, checkers[i].GetComponent<Checker>().gridX - 1 * cF, checkers[i].GetComponent<Checker>().gridY + 1 * cF, out invLeft);
                Checker hoppedRight = Checker.GetChecker(checkers, checkers[i].GetComponent<Checker>().gridX + 1 * cF, checkers[i].GetComponent<Checker>().gridY + 1 * cF, out invRight);
                Checker landLeft = Checker.GetChecker(checkers, checkers[i].GetComponent<Checker>().gridX - 2 * cF, checkers[i].GetComponent<Checker>().gridY + 2 * cF, out invLeft);
                Checker landRight = Checker.GetChecker(checkers, checkers[i].GetComponent<Checker>().gridX + 2 * cF, checkers[i].GetComponent<Checker>().gridY + 2 * cF, out invRight);

                //Left jump is invalid, check right
                if (invLeft == true)
                {
                    if (hoppedRight != null && landRight == null &&
                        ((checkers[i].GetComponent<Checker>().red == true && hoppedRight.red == false) || (checkers[i].GetComponent<Checker>().red == false && hoppedRight.red == true)))
                    {
                        //If true, we can immediately set the valid moves map for this piece and return
                        if (checkers[i] == currentGoChecker)
                        {
                            for (j = 0; j < 64; j++)
                            {
                                //Set origin to valid, but don't make it a move, rather it allows them to put the piece down
                                //without ending their turn (handled in set piece down). Also set the landing position to valid
                                if ((tiles[j].GetComponent<Tile>().gridX == currentGoTile.GetComponent<Tile>().gridX && tiles[j].GetComponent<Tile>().gridY == currentGoTile.GetComponent<Tile>().gridY) ||
                                    (tiles[j].GetComponent<Tile>().gridX == checkers[i].GetComponent<Checker>().gridX + 2 * cF && tiles[j].GetComponent<Tile>().gridY == checkers[i].GetComponent<Checker>().gridY + 2 * cF))
                                {
                                    tiles[j].GetComponent<Tile>().valid = true;
                                }
                                else
                                {
                                    tiles[j].GetComponent<Tile>().valid = false;
                                }
                            }

                            //Set the checker's origin position and pick it up
                            currentGoChecker.GetComponent<Checker>().originPos = currentGoChecker.GetComponent<Transform>().position;
                            currentGoChecker.GetComponent<Checker>().originGridX = currentGoChecker.GetComponent<Checker>().gridX;
                            currentGoChecker.GetComponent<Checker>().originGridY = currentGoChecker.GetComponent<Checker>().gridY;
                            currentGoChecker.GetComponent<Transform>().position = new Vector3(
                                currentGoChecker.GetComponent<Transform>().position.x,
                                currentGoChecker.GetComponent<Transform>().position.y + 1f,
                                currentGoChecker.GetComponent<Transform>().position.z);

                            return;
                        }
                        else
                            checkersCanJump++;
                    }
                }
                //Right jump is invalid, check left
                else if (invRight == true)
                {
                    if (hoppedLeft != null && landLeft == null &&
                        ((checkers[i].GetComponent<Checker>().red == true && hoppedLeft.red == false) || (checkers[i].GetComponent<Checker>().red == false && hoppedLeft.red == true)))
                    {
                        //If true, we can immediately set the valid moves map for this piece and return
                        if (checkers[i] == currentGoChecker)
                        {
                            for (j = 0; j < 64; j++)
                            {
                                //Set origin to valid, but don't make it a move, rather it allows them to put the piece down
                                //without ending their turn (handled in set piece down). Also set the landing position to valid
                                if ((tiles[j].GetComponent<Tile>().gridX == currentGoTile.GetComponent<Tile>().gridX && tiles[j].GetComponent<Tile>().gridY == currentGoTile.GetComponent<Tile>().gridY) ||
                                    (tiles[j].GetComponent<Tile>().gridX == checkers[i].GetComponent<Checker>().gridX - 2 * cF && tiles[j].GetComponent<Tile>().gridY == checkers[i].GetComponent<Checker>().gridY + 2 * cF))
                                {
                                    tiles[j].GetComponent<Tile>().valid = true;
                                }
                                else
                                {
                                    tiles[j].GetComponent<Tile>().valid = false;
                                }
                            }

                            //Set the checker's origin position and pick it up
                            currentGoChecker.GetComponent<Checker>().originPos = currentGoChecker.GetComponent<Transform>().position;
                            currentGoChecker.GetComponent<Checker>().originGridX = currentGoChecker.GetComponent<Checker>().gridX;
                            currentGoChecker.GetComponent<Checker>().originGridY = currentGoChecker.GetComponent<Checker>().gridY;
                            currentGoChecker.GetComponent<Transform>().position = new Vector3(
                                currentGoChecker.GetComponent<Transform>().position.x,
                                currentGoChecker.GetComponent<Transform>().position.y + 1f,
                                currentGoChecker.GetComponent<Transform>().position.z);

                            return;
                        }
                        else
                            checkersCanJump++;
                    }
                }
                //Neither are invalid, check if there is a jump in either direction
                else
                {
                    if ((hoppedRight != null && landRight == null &&
                        ((checkers[i].GetComponent<Checker>().red == true && hoppedRight.red == false) || (checkers[i].GetComponent<Checker>().red == false && hoppedRight.red == true))) ||
                        (hoppedLeft != null && landLeft == null &&
                        ((checkers[i].GetComponent<Checker>().red == true && hoppedLeft.red == false) || (checkers[i].GetComponent<Checker>().red == false && hoppedLeft.red == true))))
                    {
                        //If true, we can immediately set the valid moves map for this piece and return
                        if (checkers[i] == currentGoChecker)
                        {
                            for (j = 0; j < 64; j++)
                            {
                                //Set origin to valid, but don't make it a move, rather it allows them to put the piece down
                                //without ending their turn (handled in set piece down). Also set the landing positions to valid
                                if ((tiles[j].GetComponent<Tile>().gridX == currentGoTile.GetComponent<Tile>().gridX && tiles[j].GetComponent<Tile>().gridY == currentGoTile.GetComponent<Tile>().gridY) ||
                                    (tiles[j].GetComponent<Tile>().gridX == checkers[i].GetComponent<Checker>().gridX + 2 * cF && tiles[j].GetComponent<Tile>().gridY == checkers[i].GetComponent<Checker>().gridY + 2 * cF) ||
                                    (tiles[j].GetComponent<Tile>().gridX == checkers[i].GetComponent<Checker>().gridX - 2 * cF && tiles[j].GetComponent<Tile>().gridY == checkers[i].GetComponent<Checker>().gridY + 2 * cF))
                                {
                                    tiles[j].GetComponent<Tile>().valid = true;
                                }
                                else
                                {
                                    tiles[j].GetComponent<Tile>().valid = false;
                                }
                            }

                            //Set the checker's origin position and pick it up
                            currentGoChecker.GetComponent<Checker>().originPos = currentGoChecker.GetComponent<Transform>().position;
                            currentGoChecker.GetComponent<Checker>().originGridX = currentGoChecker.GetComponent<Checker>().gridX;
                            currentGoChecker.GetComponent<Checker>().originGridY = currentGoChecker.GetComponent<Checker>().gridY;
                            currentGoChecker.GetComponent<Transform>().position = new Vector3(
                                currentGoChecker.GetComponent<Transform>().position.x,
                                currentGoChecker.GetComponent<Transform>().position.y + 1f,
                                currentGoChecker.GetComponent<Transform>().position.z);

                            return;
                        }
                        else
                            checkersCanJump++;
                    }
                }
            }

            //Now check if there are any jumps available to the checkers
            if (checkersCanJump > 0)
            {
                for (j = 0; j < 64; j++)
                {
                    //Set origin to valid, but don't make it a move, rather it allows them to put the piece down
                    //without ending their turn (handled in set piece down).
                    if (tiles[j].GetComponent<Tile>().gridX == currentGoTile.GetComponent<Tile>().gridX && tiles[j].GetComponent<Tile>().gridY == currentGoTile.GetComponent<Tile>().gridY)
                    {
                        tiles[j].GetComponent<Tile>().valid = true;
                    }
                    else
                    {
                        tiles[j].GetComponent<Tile>().valid = false;
                    }
                }
            }

            //If no pieces can jump, map the currentChecker's potential moves
            else
            {
                for (j = 0; j < 64; j++)
                {
                    //Set origin to valid, but don't make it a move, rather it allows them to put the piece down
                    //without ending their turn (handled in set piece down).Also set empty diagnols to valid
                    if ((tiles[j].GetComponent<Tile>().gridX == currentGoTile.GetComponent<Tile>().gridX && tiles[j].GetComponent<Tile>().gridY == currentGoTile.GetComponent<Tile>().gridY) ||
                        ((tiles[j].GetComponent<Tile>().gridX == currentGoTile.GetComponent<Tile>().gridX + 1 * cF) || (tiles[j].GetComponent<Tile>().gridX == currentGoTile.GetComponent<Tile>().gridX - 1 * cF)) &&
                        (tiles[j].GetComponent<Tile>().gridY == currentGoTile.GetComponent<Tile>().gridY + 1 * cF))
                    {
                        bool invalid;
                        if (Checker.GetChecker(checkers, tiles[j].GetComponent<Tile>().gridX, tiles[j].GetComponent<Tile>().gridY, out invalid) != null)
                            tiles[j].GetComponent<Tile>().valid = false;
                        else
                            tiles[j].GetComponent<Tile>().valid = true;
                    }
                    else
                    {
                        tiles[j].GetComponent<Tile>().valid = false;
                    }
                }
            }

            //Set the checker's origin position and pick it up
            currentGoChecker.GetComponent<Checker>().originPos = currentGoChecker.GetComponent<Transform>().position;
            currentGoChecker.GetComponent<Checker>().originGridX = currentGoChecker.GetComponent<Checker>().gridX;
            currentGoChecker.GetComponent<Checker>().originGridY = currentGoChecker.GetComponent<Checker>().gridY;
            currentGoChecker.GetComponent<Transform>().position = new Vector3(
                currentGoChecker.GetComponent<Transform>().position.x,
                currentGoChecker.GetComponent<Transform>().position.y + 1f,
                currentGoChecker.GetComponent<Transform>().position.z);

            return;
        }
    }

    //[Command] //Routes an rpc call to move a checker piece; keeps the player objects on the clients and server synced
    public void MovePiece(Vector2 pos)
    {
        //RpcMovePiece(pos);
        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(pos));

        foreach (var hit in hits)
        {
            //Check if the object hit is a tile and not the current tile
            if (hit.transform.tag.Equals("Tile") && hit.collider.gameObject != currentGoTile)
            {
                //Reset the highlight of the previous tile
                currentGoTile.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
                currentGoTile.GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");

                //***Make any adjustments to the old tile above this***

                //Release the LPA over the previous tile and obtain LPA over the new tile
                CmdRemoveObjectAuthority(currentGoTile.GetComponent<NetworkIdentity>().netId);
                currentGoTile = hit.collider.gameObject;
                CmdAssignObjectAuthority(currentGoTile.GetComponent<NetworkIdentity>().netId);

                //***Make any adjustments to the new tile below this***

                //Snap the checker to hover above the new tile
                currentGoChecker.GetComponent<Transform>().position = new Vector3(currentGoTile.GetComponent<Transform>().position.x,
                    currentGoChecker.GetComponent<Transform>().position.y,
                    currentGoTile.GetComponent<Transform>().position.z);

                //Update the checker's x and y values
                currentGoChecker.GetComponent<Checker>().gridX = currentGoTile.GetComponent<Tile>().gridX;
                currentGoChecker.GetComponent<Checker>().gridY = currentGoTile.GetComponent<Tile>().gridY;

                //Set the appropriate highlight color of the new tile
                currentGoTile.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
                if (currentGoTile.GetComponent<Tile>().valid == true)
                {
                    if (currentGoTile.GetComponent<Tile>().white == true)
                        currentGoTile.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 0.25f, 0f));
                    else
                        currentGoTile.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 0.125f, 0f));
                }
                else
                {
                    if (currentGoTile.GetComponent<Tile>().white == true)
                        currentGoTile.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0.25f, 0f, 0f));
                    else
                        currentGoTile.GetComponent<Tile>().GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0.125f, 0f, 0f));
                }
            }
        }
    }

    //[Command] //Routes an rpc call to put down a checker piece; keeps the client-to-server player objects synced
    public void PutPieceDown()
    {
        //RpcPutPieceDown();
        bool invChecker;
        var checkers = GameObject.FindGameObjectsWithTag("Checker");

        //Put down the checker if the position is valid, otherwise snap back to original position
        if (currentGoTile.GetComponent<Tile>().valid == false)
        {
            currentGoChecker.GetComponent<Transform>().position = currentGoChecker.GetComponent<Checker>().originPos;
            currentGoChecker.GetComponent<Checker>().gridX = currentGoChecker.GetComponent<Checker>().originGridX;
            currentGoChecker.GetComponent<Checker>().gridY = currentGoChecker.GetComponent<Checker>().originGridY;
        }
        else
        {
            //Set it down
            currentGoChecker.GetComponent<Transform>().position = new Vector3(
                currentGoChecker.GetComponent<Transform>().position.x,
                currentGoChecker.GetComponent<Transform>().position.y - 1f,
                currentGoChecker.GetComponent<Transform>().position.z);

            //Delete
            if (currentGoChecker.GetComponent<Checker>().gridX == currentGoChecker.GetComponent<Checker>().originGridX + 2)
            {
                if (currentGoChecker.GetComponent<Checker>().gridY == currentGoChecker.GetComponent<Checker>().originGridY + 2)
                {
                    Checker deleteChecker = Checker.GetChecker(checkers, currentGoChecker.GetComponent<Checker>().originGridX + 1,
                        currentGoChecker.GetComponent<Checker>().originGridY + 1, out invChecker);
                    Destroy(deleteChecker);
                    enemypieces = enemypieces - 1;
                }
            }

            if (currentGoChecker.GetComponent<Checker>().gridX == currentGoChecker.GetComponent<Checker>().originGridX + 2)
            {
                if (currentGoChecker.GetComponent<Checker>().gridY == currentGoChecker.GetComponent<Checker>().originGridY - 2)
                {
                    Checker deleteChecker = Checker.GetChecker(checkers, currentGoChecker.GetComponent<Checker>().originGridX + 1,
                        currentGoChecker.GetComponent<Checker>().originGridY - 1, out invChecker);
                    Destroy(deleteChecker);
                    enemypieces = enemypieces - 1;
                }
            }

            if (currentGoChecker.GetComponent<Checker>().gridX == currentGoChecker.GetComponent<Checker>().originGridX - 2)
            {
                if (currentGoChecker.GetComponent<Checker>().gridY == currentGoChecker.GetComponent<Checker>().originGridY - 2)
                {
                    Checker deleteChecker = Checker.GetChecker(checkers, currentGoChecker.GetComponent<Checker>().originGridX - 1,
                        currentGoChecker.GetComponent<Checker>().originGridY - 1, out invChecker);
                    Destroy(deleteChecker);
                    enemypieces = enemypieces - 1;
                }
            }

            if (currentGoChecker.GetComponent<Checker>().gridX == currentGoChecker.GetComponent<Checker>().originGridX - 2)
            {
                if (currentGoChecker.GetComponent<Checker>().gridY == currentGoChecker.GetComponent<Checker>().originGridY + 2)
                {
                    Checker deleteChecker = Checker.GetChecker(checkers, currentGoChecker.GetComponent<Checker>().originGridX - 1,
                        currentGoChecker.GetComponent<Checker>().originGridY + 1, out invChecker);
                    Destroy(deleteChecker);
                    enemypieces = enemypieces - 1;
                }
            }

            //Set the end turn flag if they've moved to a new position (not origin)
            if (currentGoChecker.GetComponent<Checker>().gridX != currentGoChecker.GetComponent<Checker>().originGridX ||
                currentGoChecker.GetComponent<Checker>().gridY != currentGoChecker.GetComponent<Checker>().originGridY)
            {
                switchTurns = true;
            }
            else
                switchTurns = false;

            //Update the new origin information
            currentGoChecker.GetComponent<Checker>().originPos = currentGoChecker.GetComponent<Transform>().position;
            currentGoChecker.GetComponent<Checker>().originGridX = currentGoChecker.GetComponent<Checker>().gridX;
            currentGoChecker.GetComponent<Checker>().originGridY = currentGoChecker.GetComponent<Checker>().gridY;
        }

        //Reset the highlight of the previous tile
        currentGoTile.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
        currentGoTile.GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
    }

    [Command] //Routes an rpc call to switch the player turn; keeps client and server scenes synced
    public void CmdSwitchTurns()
    {
        //RpcSwitchTurns();
        currentGoChessboard.GetComponent<Chessboard>().isHostTurn = !(currentGoChessboard.GetComponent<Chessboard>().isHostTurn);
        switchTurns = false;
    }

    [Command] //Routes an rpc call to change scene to lobby; keeps client and server scenes synced
    public void CmdEndGame()
    {
        //RpcEndGame();
        SceneManager.LoadScene("lobby");
    }

    [ClientRpc]
    public void RpcSwitchTurns()
    {
        currentGoChessboard.GetComponent<Chessboard>().isHostTurn = !(currentGoChessboard.GetComponent<Chessboard>().isHostTurn);
        switchTurns = false;
    }

    [ClientRpc] //Switches scene to lobby, keeps client's and server's scenes synced
    public void RpcEndGame()
    {
        SceneManager.LoadScene("lobby");
    }

    public void Initialization()
    {
        var chessboard = GameObject.FindGameObjectWithTag("Chessboard");
        var checkers = GameObject.FindGameObjectsWithTag("Checker");
        var lighting = GameObject.FindGameObjectWithTag("Lighting");
        var oldTiles = GameObject.FindGameObjectsWithTag("OldTile");
        var newTiles = GameObject.FindGameObjectsWithTag("Tile");
        var imageTraget = GameObject.Find("ImageTarget").GetComponent<Transform>();
        int i;

        //Erase the original reference tiles in the chessboard and set the networked ones to be children of the image target
        for (i = 0; i < 64; i++)
        {
            Destroy(oldTiles[i]);
            newTiles[i].GetComponent<Transform>().parent = imageTraget;
        }

        //Destroy the placeholder chessboard reference
        Destroy(GameObject.Find("ChessboardReference"));

        //This will be true only for the joining player, not the host
        if (chessboard.GetComponent<Transform>().parent == null)
        {
            //Set parents to image target
            chessboard.GetComponent<Transform>().parent = imageTraget;
            foreach (var checker in checkers)
                checker.GetComponent<Transform>().parent = imageTraget;
            lighting.GetComponent<Transform>().parent = imageTraget;

            //Adjust frame scale
            chessboard.GetComponent<Transform>().localScale *= 10;
        }
    }
}