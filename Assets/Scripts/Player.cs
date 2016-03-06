using UnityEngine;
using UnityEngine.Networking;

public class Player : NetworkBehaviour
{
    [SyncVar]
    private GameObject checkerHit;
    [SyncVar]
    private GameObject tileHit;

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
                    checkerHit = hit.transform.gameObject;
                    CmdAssignObjectAuthority(checkerHit.GetComponent<NetworkIdentity>().netId);
                    if (Physics.Raycast(checkerHit.GetComponent<Transform>().position, Vector3.down, out hit) && hit.transform.tag.Equals("Tile"))
                    {
                        tileHit = hit.collider.gameObject;
                        CmdAssignObjectAuthority(tileHit.GetComponentInParent<NetworkIdentity>().netId);
                        CmdPickPieceUp(checkerHit, tileHit);
                    }
                    else
                        tileHit = null;
                }
                else
                    checkerHit = null;
                break;
            case TouchPhase.Moved:
                if (checkerHit != null && tileHit != null && hasAuthority == true)
                {
                    CmdDragPiece(checkerHit, Input.GetTouch(0).deltaPosition);
                    CmdTargetSquare(checkerHit, tileHit);
                }
                break;
            case TouchPhase.Ended:
                if (checkerHit != null && tileHit != null && hasAuthority == true)
                {
                    CmdPutPieceDown(checkerHit, tileHit);
                    CmdRemoveObjectAuthority(tileHit.GetComponentInParent<NetworkIdentity>().netId);
                    CmdRemoveObjectAuthority(checkerHit.GetComponent<NetworkIdentity>().netId);
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
    public void CmdDragPiece(GameObject checkerHit, Vector2 deltaPos)
    {
        RpcDragPiece(checkerHit, deltaPos);
    }

    [Command]
    public void CmdPickPieceUp(GameObject checkerHit, GameObject tileHit)
    {
        RpcPickPieceUp(checkerHit, tileHit);
    }

    [Command]
    public void CmdPutPieceDown(GameObject checkerHit, GameObject tileHit)
    {
        RpcPutPieceDown(checkerHit, tileHit);
    }

    [Command]
    public void CmdTargetSquare(GameObject checkerHit, GameObject tileHit)
    {
        RpcTargetSquare(checkerHit, tileHit);
    }

    [ClientRpc]
    public void RpcDragPiece(GameObject checkerHit, Vector2 deltaPos)
    {
        checkerHit.GetComponent<Transform>().position = new Vector3(checkerHit.GetComponent<Transform>().position.x + (deltaPos.x * _dragSpeed),
            checkerHit.GetComponent<Transform>().position.y,
            checkerHit.GetComponent<Transform>().position.z + (deltaPos.y * _dragSpeed));
    }

    [ClientRpc]
    public void RpcPickPieceUp(GameObject checkerHit, GameObject tileHit)
    {
        RaycastHit hit;

        checkerHit.GetComponent<Transform>().Translate(0f, 1f, 0f, Space.World);

        if (Physics.Raycast(checkerHit.GetComponent<Transform>().position, Vector3.down, out hit) && hit.transform.tag.Equals("Tile"))
        {
            tileHit = hit.collider.gameObject;
            CmdAssignObjectAuthority(tileHit.GetComponentInParent<NetworkIdentity>().netId);
        }
    }

    [ClientRpc]
    public void RpcPutPieceDown(GameObject checkerHit, GameObject tileHit)
    {
        checkerHit.GetComponent<Transform>().Translate(0f, -1f, 0f, Space.World);
        CmdRemoveObjectAuthority(tileHit.GetComponentInParent<NetworkIdentity>().netId);
    }

    [ClientRpc]
    public void RpcTargetSquare(GameObject checkerHit, GameObject tileHit)
    {
        RaycastHit hit;

        if (Physics.Raycast(checkerHit.GetComponent<Transform>().position, Vector3.down, out hit) && hit.transform.tag.Equals("Tile"))
        {
            if (tileHit == hit.collider.gameObject)
                //DynamicGI.SetEmissive(tileHit.GetComponent<Renderer>(), new Color(1f, 0.1f, 0.5f, 1.0f) * 0.5f);
                tileHit.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.1f, 0.5f, 1.0f);
            else
            {
                //DynamicGI.SetEmissive(tileHit.GetComponent<Renderer>(), new Color(0f, 0f, 0f, 0f) * 0f);
                tileHit.GetComponent<MeshRenderer>().material.color = new Color(0f, 0f, 0f, 1.0f);
                CmdRemoveObjectAuthority(tileHit.GetComponentInParent<NetworkIdentity>().netId);
                tileHit = hit.collider.gameObject;
                CmdAssignObjectAuthority(tileHit.GetComponentInParent<NetworkIdentity>().netId);
                //DynamicGI.SetEmissive(tileHit.GetComponent<Renderer>(), new Color(1f, 0.1f, 0.5f, 1.0f) * 0.5f);
                tileHit.GetComponent<MeshRenderer>().material.color = new Color(1f, 0.1f, 0.5f, 1.0f);
            }
        }
    }
}