using UnityEngine;
using System.Collections.Generic;

public static class Helpers
{
    //Takes a GameObject and iterates through its children, returning a 1D List of the results
    public static List<GameObject> GetChildren(this GameObject go)
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform tran in go.transform)
        {
            children.Add(tran.gameObject);
        }
        return children;
    }

    //Takes a GameObject and outer and inner list sizes and iterates through its children, returning a 2D List of the results
    public static List<List<GameObject>> GetChildren(this GameObject go, int outerSize, int innerSize)
    {
        if (outerSize < 1 || innerSize < 1)
            return null;

        List<List<GameObject>> children = new List<List<GameObject>>();
        int skip = 0;
        int j = 0;

        for(int i = 0; i < outerSize; i++)
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
                j++;

                if (j == innerSize)
                    break;
            }

            j = 0;
            skip = (i + 1) * innerSize;
            children.Add(innerList);
        }

        return children;
    }
}
