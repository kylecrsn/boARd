using UnityEngine;
using UnityEngine.Networking;

public class Tile : MonoBehaviour
{
    public State state;

    public enum State
    {
        Empty,          //Black tile that is empty = valid
        FullRed,        //Black tile occupied by a red checker = invalid
        FullBlack,      //Black tile occupied by a black checker = invalid
        Invalid         //White tile = invalid
    };
}