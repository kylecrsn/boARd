using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    [SyncVar] //Checker game object that the player is currently touching/dragging
    private GameObject currentGoChecker;
    [SyncVar] //Tile game object that the current checker is sitting/floating on
    private GameObject currentGoTile;

    public const float _dragSpeedFactor = 0.09f;
    public bool isHostPlayer;

    //Called when the scene begins; all objects have at this point spawned into the player's client instance
    public void Start()
    {
        if (!isLocalPlayer)
            return;

        LocalStateInitialization();
    }

    //Handle any object initialization that either can't be done by the object's OnStartClient method or is relative to the local player
    public void LocalStateInitialization()
    {
        var chessboard = GameObject.FindGameObjectWithTag("Chessboard");
        var checkers = GameObject.FindGameObjectsWithTag("Checker");
        var lighting = GameObject.FindGameObjectWithTag("Lighting");
        var oldTiles = GameObject.FindGameObjectWithTag("Chessboard").GetChildren();
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

        //Initialize the player's identity
        isHostPlayer = true;

        //This will be true only for the joining player, not the host
        if (chessboard.GetComponent<Transform>().parent == null)
        {
            //Reassign the actual identity
            isHostPlayer = false;

            //Set parents to image target
            chessboard.GetComponent<Transform>().parent = imageTraget;
            foreach (var checker in checkers)
                checker.GetComponent<Transform>().parent = imageTraget;
            lighting.GetComponent<Transform>().parent = imageTraget;

            //Adjust frame scale
            chessboard.GetComponent<Transform>().localScale *= 10;
        }
    }

    //Called each frame of the game
    void Update()
    {
        if (!isLocalPlayer)
            return;

        var isHostTurn = GameObject.FindGameObjectWithTag("Chessboard").GetComponent<Chessboard>().turn;

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
                            CmdPickPieceUp();
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
                        CmdMovePiece();
                    }
                    break;
                case TouchPhase.Ended:
                    //If a checker was being touched and has now been let go, put it back down on the board and release LPA over the checker and tile
                    if (currentGoChecker != null && currentGoTile != null && hasAuthority == true)
                    {
                        CmdPutPieceDown();
                        CmdRemoveObjectAuthority(currentGoTile.GetComponent<NetworkIdentity>().netId);
                        CmdRemoveObjectAuthority(currentGoChecker.GetComponent<NetworkIdentity>().netId);
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

    [Command] //Routes an rpc call to pick up a checker piece; keeps the player objects on the clients and server synced
    public void CmdPickPieceUp()
    {
        RpcPickPieceUp();
    }

    [Command] //Routes an rpc call to move a checker piece; keeps the player objects on the clients and server synced
    public void CmdMovePiece()
    {
        RpcMovePiece();
    }

    [Command] //Routes an rpc call to put down a checker piece; keeps the client-to-server player objects synced
    public void CmdPutPieceDown()
    {
        RpcPutPieceDown();
    }

    [ClientRpc] //Picks up a checker, performing any initialization logic; keeps the server-to-client objects synced
    public void RpcPickPieceUp()
    {
        var currentChecker = currentGoChecker.GetComponent<Checker>();
        var currentTile = currentGoTile.GetComponent<Tile>();
        var tiles = GameObject.FindGameObjectsWithTag("Tile");
        var checkers = GameObject.FindGameObjectsWithTag("Checker");
        var checkersCanJump = 0;

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
        if ((isHostPlayer == true && currentChecker.red == false) || (isHostPlayer == false && currentChecker.red == true))
        {
            //Handle mapping the valid tiles when hopping another piece
            foreach (var goChecker in checkers)
            {
                var checker = goChecker.GetComponent<Checker>();
                bool invLeft, invRight;

                //Don't calculate jumps for checkers you can't use
                if ((isHostPlayer == true && checker.red == true) || (isHostPlayer == false && checker.red == false))
                    continue;

                //Only care about if the checker can jump and land within the grid, overwrite inv's for hopped checkers since they don't matter
                //If the returned value is null and an inv variable is true, the x and y params are outside the valid grid
                //If the returned value is null and an inv variable is false, then that tile is empty since no checker is there
                Checker hoppedLeft = Checker.GetChecker(checkers, checker.gridX - 1 * cF, checker.gridY + 1 * cF, out invLeft);
                Checker hoppedRight = Checker.GetChecker(checkers, checker.gridX + 1 * cF, checker.gridY + 1 * cF, out invRight);
                Checker landLeft = Checker.GetChecker(checkers, checker.gridX - 2 * cF, checker.gridY + 2 * cF, out invLeft);
                Checker landRight = Checker.GetChecker(checkers, checker.gridX + 2 * cF, checker.gridY + 2 * cF, out invRight);

                //Left jump is invalid, check right
                if(invLeft == true)
                {
                    if(hoppedRight != null && landRight == null && 
                        ((checker.red == true && hoppedRight.red == false) || (checker.red == false && hoppedRight.red == true)))
                    {
                        //If true, we can immediately set the valid moves map for this piece and return
                        if (goChecker == currentGoChecker)
                        {
                            foreach (GameObject goTile in tiles)
                            {
                                var tile = goTile.GetComponent<Tile>();

                                //Set origin to valid, but don't make it a move, rather it allows them to put the piece down
                                //without ending their turn (handled in set piece down). Also set the landing position to valid
                                if ((tile.gridX == currentTile.gridX && tile.gridY == currentTile.gridY) ||
                                    (tile.gridX == checker.gridX + 2 * cF && tile.gridY == checker.gridY + 2 * cF))
                                {
                                    tile.valid = true;
                                }
                                else
                                {
                                    tile.valid = false;
                                }
                            }

                            //Set the checker's origin position and pick it up
                            currentChecker.originPos = currentGoChecker.GetComponent<Transform>().position;
                            currentChecker.originGridX = currentChecker.gridX;
                            currentChecker.originGridY = currentChecker.gridY;
                            currentGoChecker.GetComponent<Transform>().Translate(0f, 1f, 0f, Space.World);

                            return;
                        }
                        else
                            checkersCanJump++;
                    }
                }
                //Right jump is invalid, check left
                else if(invRight == true)
                {
                    if (hoppedLeft != null && landLeft == null &&
                        ((checker.red == true && hoppedLeft.red == false) || (checker.red == false && hoppedLeft.red == true)))
                    {
                        //If true, we can immediately set the valid moves map for this piece and return
                        if (goChecker == currentGoChecker)
                        {
                            foreach (GameObject goTile in tiles)
                            {
                                var tile = goTile.GetComponent<Tile>();

                                //Set origin to valid, but don't make it a move, rather it allows them to put the piece down
                                //without ending their turn (handled in set piece down). Also set the landing position to valid
                                if ((tile.gridX == currentTile.gridX && tile.gridY == currentTile.gridY) ||
                                    (tile.gridX == checker.gridX - 2 * cF && tile.gridY == checker.gridY + 2 * cF))
                                {
                                    tile.valid = true;
                                }
                                else
                                {
                                    tile.valid = false;
                                }
                            }

                            //Set the checker's origin position and pick it up
                            currentChecker.originPos = currentGoChecker.GetComponent<Transform>().position;
                            currentChecker.originGridX = currentChecker.gridX;
                            currentChecker.originGridY = currentChecker.gridY;
                            currentGoChecker.GetComponent<Transform>().Translate(0f, 1f, 0f, Space.World);

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
                        ((checker.red == true && hoppedRight.red == false) || (checker.red == false && hoppedRight.red == true))) ||
                        (hoppedLeft != null && landLeft == null &&
                        ((checker.red == true && hoppedLeft.red == false) || (checker.red == false && hoppedLeft.red == true))))
                    {
                        //If true, we can immediately set the valid moves map for this piece and return
                        if (goChecker == currentGoChecker)
                        {
                            foreach (GameObject goTile in tiles)
                            {
                                var tile = goTile.GetComponent<Tile>();

                                //Set origin to valid, but don't make it a move, rather it allows them to put the piece down
                                //without ending their turn (handled in set piece down). Also set the landing positions to valid
                                if ((tile.gridX == currentTile.gridX && tile.gridY == currentTile.gridY) ||
                                    (tile.gridX == checker.gridX + 2 * cF && tile.gridY == checker.gridY + 2 * cF) ||
                                    (tile.gridX == checker.gridX - 2 * cF && tile.gridY == checker.gridY + 2 * cF))
                                {
                                    tile.valid = true;
                                }
                                else
                                {
                                    tile.valid = false;
                                }
                            }

                            //Set the checker's origin position and pick it up
                            currentChecker.originPos = currentGoChecker.GetComponent<Transform>().position;
                            currentChecker.originGridX = currentChecker.gridX;
                            currentChecker.originGridY = currentChecker.gridY;
                            currentGoChecker.GetComponent<Transform>().Translate(0f, 1f, 0f, Space.World);

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
                foreach (GameObject goTile in tiles)
                {
                    var tile = goTile.GetComponent<Tile>();

                    //Set origin to valid, but don't make it a move, rather it allows them to put the piece down
                    //without ending their turn (handled in set piece down).
                    if (tile.gridX == currentTile.gridX && tile.gridY == currentTile.gridY)
                    {
                        tile.valid = true;
                    }
                    else
                    {
                        tile.valid = false;
                    }
                }
            }

            //If no pieces can jump, map the currentChecker's potential moves
            else
            {
                foreach (GameObject goTile in tiles)
                {
                    var tile = goTile.GetComponent<Tile>();

                    //Set origin to valid, but don't make it a move, rather it allows them to put the piece down
                    //without ending their turn (handled in set piece down).Also set empty diagnols to valid
                    if ((tile.gridX == currentTile.gridX && tile.gridY == currentTile.gridY) ||
                        ((tile.gridX == currentTile.gridX + 1 * cF) || (tile.gridX == currentTile.gridX - 1 * cF)) &&
                        (tile.gridY== currentTile.gridY + 1 * cF) &&
                        (tile.blackChecker == false && tile.redChecker == false))
                    {
                        tile.valid = true;
                    }
                    else
                    {
                        tile.valid = false;
                    }
                }
            }

            //Set the checker's origin position and pick it up
            currentChecker.originPos = currentGoChecker.GetComponent<Transform>().position;
            currentChecker.originGridX = currentChecker.gridX;
            currentChecker.originGridY = currentChecker.gridY;
            currentGoChecker.GetComponent<Transform>().Translate(0f, 1f, 0f, Space.World);

            return;
        }
    }

    [ClientRpc] //Moves a checker, handling all checker/tile modifications and game logic; keeps the server-to-client objects synced
    public void RpcMovePiece()
    {
        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.GetTouch(0).position));

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

    [ClientRpc] //Puts down a checker, performing any reset logic; keeps the server-to-client objects synced
    public void RpcPutPieceDown()
    {
        var currentChecker = currentGoChecker.GetComponent<Checker>();

        //Put down the checker if the position is valid, otherwise snap back to original position
        if (currentGoTile.GetComponent<Tile>().valid == false)
        {
            currentGoChecker.GetComponent<Transform>().position = currentChecker.originPos;
            currentChecker.gridX = currentChecker.originGridX;
            currentChecker.gridY = currentChecker.originGridY;
        }
        else
        {
            //Set it down
            currentGoChecker.GetComponent<Transform>().Translate(0f, -1f, 0f, Space.World);

            //Set the end turn flag if they've moved to a new position (not origin)
            if (currentChecker.gridX != currentChecker.originGridX ||
                currentChecker.gridY != currentChecker.originGridY)
            {
                GameObject.FindGameObjectWithTag("Chessboard").GetComponent<Chessboard>().turn =
                    !GameObject.FindGameObjectWithTag("Chessboard").GetComponent<Chessboard>().turn;
            }

            //Update the new origin information
            currentChecker.originPos = currentGoChecker.GetComponent<Transform>().position;
            currentChecker.originGridX = currentChecker.gridX;
            currentChecker.originGridY = currentChecker.gridY;
        }

        //Reset the highlight of the previous tile
        currentGoTile.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
        currentGoTile.GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
    }
}