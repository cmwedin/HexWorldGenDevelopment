using UnityEngine;
using UnityEngine.UI;

public class HexGridChunk : MonoBehaviour
{
    HexCell[] cells;
    HexMesh HexMesh;
    Canvas GridCanvas;

    void Awake () {
        GridCanvas = GetComponentInChildren<Canvas>();
        HexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
    }

    /*void Start () {
        HexMesh.Triangulate(cells);
    }*/

    public void AddCell (int index, HexCell cell) {
        cells[index] = cell;
        cell.chunk = this;
        cell.transform.SetParent(transform, false);
        cell.uiRect.SetParent(GridCanvas.transform, false);
    }

    public void Refresh () {
        //if (cells != null) { HexMesh.Triangulate(cells); }
        enabled = true;
    }

    void LateUpdate () {
        HexMesh.Triangulate(cells);
        enabled = false;
    }
}
