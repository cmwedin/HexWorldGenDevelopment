using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum BrushType { 
    Terrain, Paint, Rivers
}
public class HexMeshBrush : MonoBehaviour
{
    //public Color[] colors;
    [HideInInspector] public Color activeColor;
    [HideInInspector] public int activeElevation = 1;
    [HideInInspector] public int brushSize = 1;
    [HideInInspector] public bool removeRivers = false;
    private BrushType brushType;


    public HexGrid hexGrid;
    bool applyColor;
    bool applyElevation;
    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;

    void Update()
    {
        if (  Input.GetMouseButton(0) 
            && !EventSystem.current.IsPointerOverGameObject()) 
            { HandleInput(); }
        else { previousCell = null; }
    }

    void HandleInput() {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit)) {
            HexCell currentCell = hexGrid.GetCell(hit.point);
            if (previousCell && previousCell != currentCell) { ValidateDrag(currentCell); }
            else { isDrag = false; }
            EditCells(currentCell);
            previousCell = currentCell;
        } else { previousCell = null; }
    }

    public void SetType(BrushType _brushType)
    {
        brushType = _brushType;
    }

    void ValidateDrag(HexCell currentCell) {
        for (dragDirection = HexDirection.NE;
             dragDirection <= HexDirection.NW;
             dragDirection++
        ) {
            if (previousCell.GetNeighbor(dragDirection) == currentCell) {
                isDrag = true;
                return;
            }

        }
        isDrag = false;
    }

    void EditCells ( HexCell center) {
        int centerX = center.coords.X;
        int centerZ = center.coords.Z;

        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++,  r++) {
            for (int x = centerX - r; x <= centerX + brushSize; x++) {
                EditCell(hexGrid.GetCell(new HexCoords(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
            for (int x = centerX - brushSize; x <= centerX + r; x++) {
                EditCell(hexGrid.GetCell(new HexCoords(x, z)));
            }
        }
    }

    void EditCell (HexCell cell) {
        if (cell) {
            switch (brushType) {
                case BrushType.Terrain:
                    cell.Elevation = activeElevation;
                    break;
                case BrushType.Paint:
                    cell.color = activeColor;
                    break;
                case BrushType.Rivers:
                    if(removeRivers) {cell.RemoveRiver();}
                    else if(isDrag) {
                        HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                        if (otherCell) otherCell.SetOutgoingRiver(dragDirection);
                    }
                    break;
                default: 
                    Debug.LogWarning("Brush type not set");
                    break;
            }
        }
    }
}
