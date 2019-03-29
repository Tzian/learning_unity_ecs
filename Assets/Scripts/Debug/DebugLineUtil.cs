using System.Collections.Generic;
using UnityEngine;

public struct DebugLineUtil
{
    int sectorSize;
    Vector3 [] cubeVectors;

    public DebugLineUtil (int sectorSize)
    {
        this.sectorSize = sectorSize;
        this.cubeVectors = CubeVectors ();
    }

    public struct DebugLine
    {
        public readonly Vector3 a, b;
        public readonly Color c;
        public DebugLine (Vector3 a, Vector3 b, Color c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }
    }

    static Vector3 [] CubeVectors ()
    {
        return new Vector3 [] {  new Vector3(0, 0, 1),	//	left front bottom
	                            new Vector3(1, 0, 0),	//	right back bottom
	                            new Vector3(0, 0, 0), 	//	left back bottom
	                            new Vector3(1, 0, 1),	//	right front bottom
	                            new Vector3(0, 0, 1),	//	left front top
	                            new Vector3(1, 0, 0),	//	right back top
	                            new Vector3(0, 0, 0),	//	left back top
	                            new Vector3(1, 0, 1) };	//	right front top
    }

    public List<DebugLine> CreateBox (Vector3 position, float size, Color color, bool noSides = false, bool topOnly = false)
    {
        Vector3 [] v = new Vector3 [cubeVectors.Length];
        //  Offset to center cubes smaller than squareWidth
        Vector3 offsetAll = position;// + (Vector3.one * ((squareWidth - size)/2));
        for (int i = 0; i < cubeVectors.Length; i++)
        {
            //  Set size and offset
            Vector3 vector = ((cubeVectors [i] * size) + offsetAll);
            v [i] = vector;
        }

        List<DebugLine> lines = new List<DebugLine> ();

        //  Top square
        lines.Add (new DebugLine (v [4], v [6], color));
        lines.Add (new DebugLine (v [5], v [7], color));
        lines.Add (new DebugLine (v [4], v [7], color));
        lines.Add (new DebugLine (v [5], v [6], color));

        if (topOnly) return lines;

        //  Bottom square
        lines.Add (new DebugLine (v [0], v [2], color));
        lines.Add (new DebugLine (v [1], v [3], color));
        lines.Add (new DebugLine (v [0], v [3], color));
        lines.Add (new DebugLine (v [1], v [2], color));

        if (noSides) return lines;

        //  Connecting lines at corners
        lines.Add (new DebugLine (v [0], v [4], color));
        lines.Add (new DebugLine (v [1], v [5], color));
        lines.Add (new DebugLine (v [2], v [6], color));
        lines.Add (new DebugLine (v [3], v [7], color));

        return lines;
    }
}