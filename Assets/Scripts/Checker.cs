using UnityEngine;
using UnityEngine.Networking;

public class Checker : NetworkBehaviour
{
    [SyncVar] //True if the checker is red, false if it is black
    public bool red;
    [SyncVar] //Original position of the checker when it was picked up, used in calculating valid move locations
    public Vector3 originPos;
    [SyncVar] //X coodinate of the checker in the checkerboard grid (ranges from 0 to 7)
    public int gridX;
    [SyncVar] //Y coodinate of the checker in the checkerboard grid (ranges from 0 to 7)
    public int gridY;
    [SyncVar] //Origin X coodinate of the checker in the checkerboard grid (ranges from 0 to 7)
    public int originGridX;
    [SyncVar] //Origin Y coodinate of the checker in the checkerboard grid (ranges from 0 to 7)
    public int originGridY;

    //Called when the client connects to the server, after its SyncVars have been initialized
    public override void OnStartClient()
    {
        if (!isClient)
            return;

        var materialPlasticBlack = (Material)Resources.Load("plastic-black");
        var materialPlasticRed = (Material)Resources.Load("plastic-red");

        //Assign the checker's material
        if (this.red == true)
            this.GetComponent<MeshRenderer>().material = materialPlasticRed;
        else
            this.GetComponent<MeshRenderer>().material = materialPlasticBlack;
    }

    //Returns a Checker GameObject from the given array of checker objects of the given x and y corrdinates
    public static Checker GetChecker(GameObject[] checkers, int x, int y, out bool invalid)
    {
        if (x < 0 || x > 7 || y < 0 || y > 7)
        {
            invalid = true;
            return null;
        }

        invalid = false;
        foreach (var checker in checkers)
        {
            if (checker.GetComponent<Checker>().gridX == x && checker.GetComponent<Checker>().gridY == y)
                return checker.GetComponent<Checker>();
        }

        return null;
    }
}