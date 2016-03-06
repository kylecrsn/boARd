using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{

    [SyncVar]
    private GameObject currentChecker;
    [SyncVar]
    private GameObject currentTile;

    public const float _dragSpeed = 0.09f;

    public void Start()
    {
        if (!isLocalPlayer)
            return;

        LocalStateInitialization();
    }

    public void LocalStateInitialization()
    {
        //Erase the original tiles in the chessboard and set the new ones to be children of the image target
        var oldTiles = GameObject.FindGameObjectWithTag("Chessboard").GetChildren();
        var newTiles = GameObject.FindGameObjectsWithTag("Tile");
        int i;

        for (i = 0; i < 64; i++)
        {
            Destroy(oldTiles[i]);
            newTiles[i].GetComponent<Transform>().parent = GameObject.Find("ImageTarget").GetComponent<Transform>();
        }

        Destroy(GameObject.Find("ChessboardReference"));
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        if (Input.touchCount == 1)
        {
            AttemptMoveChecker();
        }
    }

    //Called for one touch, handles picking up, dragging, and letting go of checker
    private void AttemptMoveChecker()
    {
        RaycastHit hit;

        switch (Input.GetTouch(0).phase)
        {
            case TouchPhase.Began:
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out hit) && hit.transform.tag.Equals("Checker"))
                {
                    currentChecker = hit.transform.gameObject;
                    CmdAssignObjectAuthority(currentChecker.GetComponent<NetworkIdentity>().netId);
                    if (Physics.Raycast(currentChecker.GetComponent<Transform>().position, Vector3.down, out hit) && hit.transform.tag.Equals("Tile"))
                    {
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
                if (currentChecker != null && currentTile != null && hasAuthority == true)
                {
                   // CmdDragPiece(Input.GetTouch(0).deltaPosition);
                    CmdTargetSquare();
                }
                break;
            case TouchPhase.Ended:
                if (currentChecker != null && currentTile != null && hasAuthority == true)
                {
                    CmdPutPieceDown();
                    CmdRemoveObjectAuthority(currentTile.GetComponent<NetworkIdentity>().netId);
                    CmdRemoveObjectAuthority(currentChecker.GetComponent<NetworkIdentity>().netId);
                }
                break;
        }
    }

    [Command]
    void CmdAssignObjectAuthority(NetworkInstanceId netId)
    {
        NetworkServer.objects[netId].AssignClientAuthority(connectionToClient);
    }

    [Command]
    void CmdRemoveObjectAuthority(NetworkInstanceId netId)
    {
        NetworkServer.objects[netId].RemoveClientAuthority(connectionToClient);
    }

    [Command]
    public void CmdDragPiece(Vector2 deltaPos)
    {
        RpcDragPiece(deltaPos);
    }

    [Command]
    public void CmdPickPieceUp()
    {
        RpcPickPieceUp();
    }

    [Command]
    public void CmdPutPieceDown()
    {
        RpcPutPieceDown();
    }

    [Command]
    public void CmdTargetSquare()
    {
        RpcTargetSquare();
    }

    [ClientRpc]
    public void RpcDragPiece(Vector2 deltaPos)
    {
        currentChecker.GetComponent<Transform>().position = new Vector3(currentChecker.GetComponent<Transform>().position.x + (deltaPos.x * _dragSpeed),
            currentChecker.GetComponent<Transform>().position.y,
            currentChecker.GetComponent<Transform>().position.z + (deltaPos.y * _dragSpeed));
    }

    [ClientRpc]
    public void RpcPickPieceUp()
    {
        currentChecker.GetComponent<Transform>().Translate(0f, 1f, 0f, Space.World);
    }

    [ClientRpc]
    public void RpcPutPieceDown()
    {
        currentChecker.GetComponent<Transform>().Translate(0f, -1f, 0f, Space.World);
    }

    [ClientRpc]
    public void RpcTargetSquare()
    {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out hit) && hit.transform.tag.Equals("Tile"))
        {
            if (currentTile != hit.collider.gameObject)
            {
                currentTile.GetComponent<MeshRenderer>().material.DisableKeyword("_EMISSION");

                //////
                CmdRemoveObjectAuthority(currentTile.GetComponent<NetworkIdentity>().netId);
                currentTile = hit.collider.gameObject;
                CmdAssignObjectAuthority(currentTile.GetComponent<NetworkIdentity>().netId);
                //////

                currentChecker.GetComponent<Transform>().position = new Vector3(currentTile.GetComponent<Transform>().position.x,
                    currentChecker.GetComponent<Transform>().position.y,
                    currentTile.GetComponent<Transform>().position.z);

                currentTile.GetComponent<MeshRenderer>().material.EnableKeyword("_EMISSION");
                currentTile.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0.25f, 0f, 0f));
            }
        }
    }
}