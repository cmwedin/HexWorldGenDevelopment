using System.Diagnostics;
using UnityEngine;

[System.Serializable]

public struct HexCoords {
    [SerializeField]
    private int x, z;
    
    public int X { get { return x; } }
    public int Z { get { return z; } }
    public int Y { get { return -X - Z; } }

    public HexCoords (int x, int z) {
        this.x = x;
        this.z = z;
    }

    public static HexCoords FromOffsetCoords (int x, int z) {
        return new HexCoords(x - z / 2, z);
    }

    public static HexCoords FromPosition(Vector3 pos) {
        float x = pos.x / (HexMetrics.innerRadius * 2f);
        float y = -x;
        float offset = pos.z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x -y);

        if(iX + iY + iZ != 0) { //UnityEngine.Debug.LogWarning("rounding error!"); 
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x -y - iZ);
            if (dX > dY && dX > dZ) { iX = -iY - iZ; }
            else if (dZ > dY) { iZ = -iX - iY; }
        }

        return new HexCoords(iX, iZ);
    }

    public override string ToString() {
        return "(" + X.ToString() + "," + Y.ToString() + "," + Z.ToString() + ")";
    }

    public string ToStringSepLines () {
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
    }
}
