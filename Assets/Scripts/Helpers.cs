using UnityEngine;
using System.Collections.Generic;

public static class Helpers
{
    //Takes a game object and iterates through its children, returning a one-dimensional list of children
    public static List<GameObject> GetChildren(this GameObject go)
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform tran in go.transform)
        {
            children.Add(tran.gameObject);
        }
        return children;
    }

    //Takes a game object and iterates through its children, using an outer and inner dimension length to return a two-dimensional list of children
    public static List<List<GameObject>> GetChildren(this GameObject go, int outerSize, int innerSize)
    {
        if (outerSize < 1 || innerSize < 1)
            return null;

        List<List<GameObject>> children = new List<List<GameObject>>();
        int skip = 0;
        int x, y = 0;

        for(x = 0; x < outerSize; x++)
        {
            var innerList = new List<GameObject>();

            foreach (Transform tran in go.transform)
            {
                if(skip > 0)
                {
                    skip--;
                    continue;
                }

                innerList.Add(tran.gameObject);
                y++;

                if (y == innerSize)
                    break;
            }

            y = 0;
            skip = (x + 1) * innerSize;
            children.Add(innerList);
        }

        return children;
    }
}
