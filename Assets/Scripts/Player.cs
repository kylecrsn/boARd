using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    [SyncVar] //Checker game object that the player is currently touching/dragging; values are maintained after letting go until the screen is touched again
    private GameObject currentChecker;
    [SyncVar] //Tile game object that the current checker is sitting/floating on; values are maintained after letting go until the screen is touched again
    private GameObject currentTile;
    [SyncVar] //Valid tile to the left diagonal
    public GameObject tilePrefab;
    [SyncVar] //Server's (black) turn when true, client's (red) turn when false
    public bool turn = true;

    public const float _dragSpeedFactor = 0.09f;

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
        var oldTiles = GameObject.FindGameObjectWithTag("Chessboard").GetChildren();
        var newTiles = GameObject.FindGameObjectsWithTag("Tile");
        int i;

        //Erase the original reference tiles in the chessboard and set the networked ones to be children of the image target
        for (i = 0; i < 64; i++)
        {
            Destroy(oldTiles[i]);
            newTiles[i].GetComponent<Transform>().parent = GameObject.Find("ImageTarget").GetComponent<Transform>();
        }

        //Destroy the placeholder chessboard reference
        Destroy(GameObject.Find("ChessboardReference"));
    }

    //Called each frame of the game
    void Update()
    {
        if (!isLocalPlayer)
            return;
        //Return if it is not your turn
        if ((!turn && isServer) || (turn && !isServer)) 
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
                        currentChecker = hit.transform.gameObject;
                        CmdAssignObjectAuthority(currentChecker.GetComponent<NetworkIdentity>().netId);
                        if (Physics.Raycast(currentChecker.GetComponent<Transform>().position, Vector3.down, out hit) && hit.transform.tag.Equals("Tile"))
                        {
                            //Assign currentTile to the tile below the checker and obtain local player authority (LPA) over it
                            currentTile = hit.collider.gameObject;
                            CmdAssignObjectAuthority(currentTile.GetComponent<NetworkIdentity>().netId);
                            CmdPickPieceUp();
                        }
                        else
                            currentTile = null;
                    }
                    else
                        currentChecker = null;
                    break;
                case TouchPhase.Moved:
                    //If a checker is currently being touched, move it and handle all necessary gameplay logic
                    if (currentChecker != null && currentTile != null && hasAuthority == true)
                    {
                        CmdMovePiece();
                    }
                    break;
                case TouchPhase.Ended:
                    //If a checker was being touched and has now been let go, put it back down on the board and release LPA over the checker and tile
                    if (currentChecker != null && currentTile != null && hasAuthority == true)
                    {
                        CmdPutPieceDown();
                        CmdRemoveObjectAuthority(currentTile.GetComponent<NetworkIdentity>().netId);
                        CmdRemoveObjectAuthority(currentChecker.GetComponent<NetworkIdentity>().netId);
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
        var newTiles = GameObject.FindGameObjectsWithTag("Tile");
        //Allows server to only move black pieces, client to only move red pieces
        if ((isServer && currentChecker.GetComponent<Checker>().red == false) ||
            (!isServer && currentChecker.GetComponent<Checker>().red == true))
        {
            //Set valid moves
            if (currentChecker.GetComponent<Checker>().red == false) //black checker movement
            {
                foreach (GameObject vTile in newTiles)
                {
                    if (((vTile.GetComponent<Tile>().gridX == currentTile.GetComponent<Tile>().gridX + 1) ||
                      (vTile.GetComponent<Tile>().gridX == currentTile.GetComponent<Tile>().gridX - 1)) &&
                      (vTile.GetComponent<Tile>().gridY == currentTile.GetComponent<Tile>().gridY + 1))
                        vTile.GetComponent<Tile>().valid = true;
                }
            }
            else if (currentChecker.GetComponent<Checker>().red == true) //red checker movement
            {
                foreach (GameObject vTile in newTiles)
                {
                    if (((vTile.GetComponent<Tile>().gridX == currentTile.GetComponent<Tile>().gridX + 1) ||
                      (vTile.GetComponent<Tile>().gridX == currentTile.GetComponent<Tile>().gridX - 1)) &&
                      (vTile.GetComponent<Tile>().gridY == currentTile.GetComponent<Tile>().gridY - 1))
                        vTile.GetComponent<Tile>().valid = true;
                }
            }
        }
            //Set the checker's origin position and pick it up
            currentChecker.GetComponent<Checker>().originPos = currentChecker.GetComponent<Transform>().position;
            currentChecker.GetComponent<Transform>().Translate(0f, 1f, 0f, Space.World);
    }

    [ClientRpc] //Moves a checker, handling all checker/tile modifications and game logic; keeps the server-to-client objects synced
    public void RpcMovePiece()
    {
        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.GetTouch(0).position));

        foreach(var hit in hits)
        {
            //Check if the object hit is a tile and not the current tile
            if (hit.transform.tag.Equals("Tile") && hit.collider.gameObject != currentTile)
            {
                //Reset the highlight of the previous tile
                currentTile.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
                currentTile.GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");

                //***Make any adjustments to the old tile above this***

                //Release the LPA over the previous tile and obtain LPA over the new tile
                CmdRemoveObjectAuthority(currentTile.GetComponent<NetworkIdentity>().netId);
                currentTile = hit.collider.gameObject;
                CmdAssignObjectAuthority(currentTile.GetComponent<NetworkIdentity>().netId);

                //***Make any adjustments to the new tile below this***

                //Snap the checker to hover above the new tile
                currentChecker.GetComponent<Transform>().position = new Vector3(currentTile.GetComponent<Transform>().position.x,
                    currentChecker.GetComponent<Transform>().position.y,
                    currentTile.GetComponent<Transform>().position.z);

                //Set the appropriate highlight color of the new tile
                currentTile.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
                if (currentTile.GetComponent<Tile>().valid == true)
                {
                    if (currentTile.GetComponent<Tile>().white == true)
                        currentTile.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 0.25f, 0f));
                    else
                        currentTile.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 0.125f, 0f));
                }
                else
                {
                    if (currentTile.GetComponent<Tile>().white == true)
                        currentTile.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0.25f, 0f, 0f));
                    else
                        currentTile.GetComponent<Tile>().GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0.125f, 0f, 0f));
                }
            }
        }
    }

    [ClientRpc] //Puts down a checker, performing any reset logic; keeps the server-to-client objects synced
    public void RpcPutPieceDown()
    {
        //Put down the checker if the position is valid, otherwise snap back to original position
        if (currentTile.GetComponent<Tile>().GetComponent<Tile>().valid == false)
            currentChecker.GetComponent<Transform>().position = currentChecker.GetComponent<Checker>().originPos;
        else
        {
            currentChecker.GetComponent<Transform>().Translate(0f, -1f, 0f, Space.World);
            turn = !turn; //End turn, need to edit for multiple turns
        }

        //Reset the highlight of the previous tile
        currentTile.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0f, 0f, 0f));
        currentTile.GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");
    }
}