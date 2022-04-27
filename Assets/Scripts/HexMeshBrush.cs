using System;
using UnityEngine;
using UnityEngine.EventSystems;

public enum BrushType { 
    Terrain, Paint, Rivers
}
public class HexMeshBrush : MonoBehaviour
{
    public Color[] colors;
    private Color activeColor;
    [HideInInspector] public int activeElevation;
    [HideInInspector] public int brushSize;
    private BrushType brushType;


    public HexGrid hexGrid;
    bool applyColor;
    bool applyElevation;
    bool isDrag;
    HexDirection dragDirection;
    HexCell previousCell;

    enum OptionalToggle { Ignore, Yes, No }
    OptionalToggle riverMode;


    void Awake () {
        SelectColor(0);
    }

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
        /* if (cell)
        {
            if (applyColor) { cell.color = activeColor; }
            if (applyElevation) cell.Elevation = activeElevation;
            if (riverMode == OptionalToggle.No) { cell.RemoveRiver(); }
            else if (isDrag && riverMode == OptionalToggle.Yes) {
                HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
                if (otherCell) otherCell.SetOutgoingRiver(dragDirection); }
        }*/
        switch (brushType) {
            case BrushType.Terrain:
                cell.Elevation = activeElevation;
                break;
            case BrushType.Paint:
                cell.color = activeColor;
                break;
            case BrushType.Rivers:
                break;
            default: break;
        }
    }

    public void SelectColor (int index) {
        applyColor = index >= 0;
        if (applyColor) { activeColor = colors[index]; }
        //UnityEngine.Debug.Log("active color is " + activeColor.ToString());
    }
}
